#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.CardReservation;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using OnAfterTuneDelegate = Mediaportal.TV.Server.TVService.Interfaces.CardHandler.OnAfterTuneDelegate;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public class CardTuner : ICardTuner
  {
    private readonly ITvCardHandler _cardHandler;

    private readonly List<ICardTuneReservationTicket> _reservationsForTune = new List<ICardTuneReservationTicket>();
    private readonly List<ICardStopReservationTicket> _reservationsForStop = new List<ICardStopReservationTicket>();

    private ICardTuneReservationTicket _activeCardTuneReservationTicket = null;
    private readonly object _cardReservationsLock = new object();    
    
    private CardTuneState _cardTuneState = CardTuneState.Idle;
    private CardStopState _cardStopState = CardStopState.Idle;    


    /// <summary>
    /// Initializes a new instance of the <see cref="CardTuner"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public CardTuner(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
      _cardHandler.Card.OnNewSubChannelEvent += new OnNewSubChannelDelegate(Card_OnNewSubChannelEvent);
    }

    private void Card_OnNewSubChannelEvent(int id)
    {
      if (_cardHandler.Tuner.HasActiveCardTuneReservationTicket)
      {
        _cardHandler.Tuner.ActiveCardTuneReservationTicket.PendingSubchannel = id;
      }
      AddTuneEvent();
    }

    public bool HasActiveCardTuneReservationTicket
    {
      get { return ActiveCardTuneReservationTicket != null; }
    }

    public object CardReservationsLock
    {
      get { return _cardReservationsLock; }
    }

    public CardTuneState CardTuneState
    {
      get { return _cardTuneState; }
      set { _cardTuneState = value; }
    }

    public ICardTuneReservationTicket ActiveCardTuneReservationTicket
    {
      get { return _activeCardTuneReservationTicket; }
      set { _activeCardTuneReservationTicket = value; }
    }

    public List<ICardTuneReservationTicket> ReservationsForTune
    {
      get { return _reservationsForTune; }
    }

    public CardStopState CardStopState
    {
      get { return _cardStopState; }
      set { _cardStopState = value; }
    }

    public List<ICardStopReservationTicket> ReservationsForStop
    {
      get { return _reservationsForStop; }
    }        

    /// <summary>
    /// Scans the the specified card to the channel.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The channel id</param>
    /// <returns></returns>
    public TvResult Scan(ref IUser user, IChannel channel, int idChannel)
    {
      ITvSubChannel result = null;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return TvResult.CardIsDisabled;
        this.LogInfo("card: Scan {0} to {1}", _cardHandler.DataBaseCard.CardId, channel.Name);

        // fix mantis 0002776: Code locking in cardtuner can cause hangs 
        //lock (this)
        {
          TvResult tvResult = TvResult.UnknownError;
          if (!BeforeTune(channel, ref user, out tvResult, idChannel))
          {
            return tvResult;
          }
          result = _cardHandler.Card.Tune(_cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel), channel);
          if (result != null)
          {
            return AfterTune(user, idChannel, result);
          }
          else
          {
            return TvResult.UnknownError;
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        if (result != null)
        {
          _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        }

        if (ex is TvExceptionNoSignal)
        {
          return TvResult.NoSignalDetected;
        }
        else if (ex is TvExceptionSWEncoderMissing)
        {
          return TvResult.SWEncoderMissing;
        }
        else if (ex is TvExceptionTunerLoadFailed)
        {
          return TvResult.GraphBuildingFailed;
        }
        else if (ex is TvExceptionNoPMT)
        {
          return TvResult.NoPmtFound;
        }
        else if (ex is TvExceptionTuneCancelled)
        {
          return TvResult.TuneCancelled;
        }
        else if (ex is TvExceptionServiceNotRunning)
        {
          return TvResult.ChannelNotActive;
        }
        return TvResult.UnknownError;
      }
    }

    private readonly IDictionary<int,ManualResetEvent> _tuneEvents = new Dictionary<int, ManualResetEvent>();
    private readonly object _tuneEvtLock = new object();

    public void CancelTune(int subchannel)
    {
      if (_cardHandler.DataBaseCard.Enabled == false || subchannel < 0)
      {
        return;
      }
      this.LogInfo("card: CancelTune {0} for subchannel {1}", _cardHandler.DataBaseCard.CardId, subchannel);
      _cardHandler.Card.CancelTune(subchannel);
      RaiseOnAfterCancelTuneEvent(subchannel);
      WaitForCancelledTuneToFinish(subchannel);
    }

    private void RaiseOnAfterCancelTuneEvent(int subchannel)
    {
      if (OnAfterCancelTuneEvent != null)
      {
        OnAfterCancelTuneEvent(subchannel);
      }
    }

    private void RemoveTuneEvent(ManualResetEvent tuneEvt, int subchannel)
    {
      if (tuneEvt != null)
      {
        lock (_tuneEvtLock)
        {
          this.LogInfo("card: RemoveTuneEvent subch: {0}", subchannel);
          tuneEvt.Close();
          _tuneEvents.Remove(subchannel);
        }
      }
    }

    private int GetPendingSubchannel()
    {
      int pendingSubchannel = -1;
      if (_cardHandler.Tuner.HasActiveCardTuneReservationTicket)
      {
        pendingSubchannel = _cardHandler.Tuner.ActiveCardTuneReservationTicket.PendingSubchannel;
      }
      return pendingSubchannel;
    }

    private void AddTuneEvent()
    {
      int subchannel = GetPendingSubchannel();
      if (subchannel > -1)
      {
        lock (_tuneEvtLock)
        {
          this.LogInfo("card: AddTuneEvent card: {0} / subch: {1}", _cardHandler.DataBaseCard.CardId, subchannel);
          _tuneEvents[subchannel] = new ManualResetEvent(false);
        }
      }
    }

    private void SignalTuneEvent(int subchannel)
    {     
      if (subchannel > -1)
      {
        lock (_tuneEvtLock)
        {
          ManualResetEvent tuneEvt;
          bool hasTuneEvt = _tuneEvents.TryGetValue(subchannel, out tuneEvt);
          if (hasTuneEvt && tuneEvt != null)
          {
            this.LogInfo("card: SignalTuneEvent card: {0} / subch: {1}", _cardHandler.DataBaseCard.CardId, subchannel);            
            tuneEvt.Set();
          }
        }
      }
    }

    private void WaitForCancelledTuneToFinish(int subchannel)
    {
      ManualResetEvent tuneEvt;
      bool hasTuneEvt;
      lock (_tuneEvtLock)
      {
        hasTuneEvt = _tuneEvents.TryGetValue(subchannel, out tuneEvt);
      }
      if (hasTuneEvt && tuneEvt != null)
      {
        this.LogInfo("card: WaitForCancelledTuneToFinish card: {0} / subch: {1}", _cardHandler.DataBaseCard.CardId, subchannel);                    
        tuneEvt.WaitOne();
        RemoveTuneEvent(tuneEvt, subchannel);
      }
    }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The channel id</param>
    /// <returns></returns>
    public TvResult Tune(ref IUser user, IChannel channel, int idChannel)
    {
      ITvSubChannel result = null;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return TvResult.CardIsDisabled;
        }
        this.LogInfo("card: Tune on card {0} to subchannel {1}", _cardHandler.DataBaseCard.CardId, channel.Name);

        int previousTimeShiftingSubChannel = _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name);   
        TvResult tvResult = TvResult.UnknownError;
        if (!BeforeTune(channel, ref user, out tvResult, idChannel))
        {
          return tvResult;
        }
        user.FailedCardId = -1;         
        result = _cardHandler.Card.Tune(previousTimeShiftingSubChannel, channel);

        if (result != null)
        {
          return AfterTune(user, idChannel, result);
        }
        return TvResult.UnknownError;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        if (result != null)
        {
          _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        }

        if (ex is TvExceptionNoSignal)
        {
          return TvResult.NoSignalDetected;
        }
        else if (ex is TvExceptionSWEncoderMissing)
        {
          return TvResult.SWEncoderMissing;
        }
        else if (ex is TvExceptionTunerLoadFailed)
        {
          return TvResult.GraphBuildingFailed;
        }
        else if (ex is TvExceptionNoPMT)
        {
          return TvResult.NoPmtFound;
        }
        else if (ex is TvExceptionTuneCancelled)
        {
          return TvResult.TuneCancelled;
        }
        else if (ex is TvExceptionServiceNotRunning)
        {
          return TvResult.ChannelNotActive;
        }
        return TvResult.UnknownError;
      }
    }

    public void CleanUpPendingTune(int pendingSubchannel)
    {
      SignalTuneEvent(pendingSubchannel);      
    }

    private TvResult AfterTune(IUser user, int idChannel, ITvSubChannel result)
    {
      bool isLocked = _cardHandler.Card.IsTunerLocked;
      this.LogDebug("card: Tuner locked: {0}", isLocked);

      this.LogInfo("**************************************************");
      this.LogInfo("***** SIGNAL LEVEL: {0}, SIGNAL QUALITY: {1} *****", _cardHandler.Card.SignalLevel,
               _cardHandler.Card.SignalQuality);
      this.LogInfo("**************************************************");

      if (result != null)
      {
        this.LogDebug("card: tuned user: {0} subchannel: {1}", user.Name, result.SubChannelId);
        _cardHandler.UserManagement.AddSubChannelOrUser(user, idChannel, result.SubChannelId);
      }
      else
      {
        return TvResult.AllCardsBusy;
      }

      if (result.IsTimeShifting || result.IsRecording)
      {
        _cardHandler.UserManagement.OnZap(user, idChannel);
      }
      return TvResult.Succeeded;
    }

    private bool BeforeTune(IChannel channel, ref IUser user, out TvResult result, int idChannel)
    {
      result = TvResult.UnknownError;
      //@FIX this fails for back-2-back recordings
      //if (CurrentDbChannel(ref user) == idChannel && idChannel >= 0)
      //{
      //  return true;
      //}
      this.LogDebug("card: user: {0}:{1}:{2} tune {3}", user.Name, user.CardId, _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel), channel.ToString());

      //check if transponder differs
      if (!IsTunedToTransponder(channel))
      {          
        if (_cardHandler.UserManagement.HasUserHighestPriority(user) || _cardHandler.UserManagement.IsOwner(user.Name) && _cardHandler.UserManagement.HasUserEqualOrHigherPriority(user))
        {
          this.LogDebug("card: to different transponder");

          //remove all subchannels, except for this user...
          int i = 0;
          IEnumerable<IUser> usersRec = _cardHandler.UserManagement.GetUsersCopy(UserType.Scheduler);

          foreach (IUser recUser in usersRec)
          {
            if (recUser.Name != user.Name)
            {
              this.LogDebug("  stop subchannel: {0} user: {1}", i, recUser.Name);
              //fix for b2b mantis; http://mantis.team-mediaportal.com/view.php?id=1112
                
              // if we are stopping an on-going recording/schedule (=admin), we have to make sure that we remove the schedule also.                
              this.LogDebug("user is scheduler: {0}", recUser.Name);
              int recScheduleId = ServiceManager.Instance.InternalControllerService.GetRecordingSchedule(recUser.CardId, recUser.Name);

              if (recScheduleId > 0)
              {
                Schedule schedule = ScheduleManagement.GetSchedule(recScheduleId);
                this.LogInfo("removing schedule with id: {0}", schedule.ScheduleId);
                ServiceManager.Instance.InternalControllerService.StopRecordingSchedule(schedule.ScheduleId);
                ScheduleManagement.DeleteSchedule(schedule.ScheduleId);
              }
              i++;
            }
          }
          //iterate all timesh. subchannels, and remove those.                            
          FreeAllTimeshiftingSubChannels(ref user);              
        }
      }
      else // same transponder, free previous subchannel before tuning..
      {          
        FreeAllTimeshiftingSubChannels(ref user);
      }

      if (OnBeforeTuneEvent != null)
      {
        OnBeforeTuneEvent(_cardHandler);
      }

      _cardHandler.Card.OnAfterTuneEvent -= new Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.OnAfterTuneDelegate(CardTuner_OnAfterTuneEvent);
      _cardHandler.Card.OnAfterTuneEvent += new Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.OnAfterTuneDelegate(CardTuner_OnAfterTuneEvent);

      result = TvResult.Succeeded;
      return true;
    }

    public void FreeAllTimeshiftingSubChannels(ref IUser user)
    {
      foreach (ISubChannel subch in user.SubChannels.Values)
      {
        if (subch.TvUsage == TvUsage.Timeshifting)
        {
          _cardHandler.Card.FreeSubChannel(subch.Id);
          int subChannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, subch.ChannelId);
          _cardHandler.UserManagement.RemoveChannelFromUser(user, subChannelId);
        }
      }
    }

    public event OnAfterCancelTuneDelegate OnAfterCancelTuneEvent;
    public event OnAfterTuneDelegate OnAfterTuneEvent;
    public event OnBeforeTuneDelegate OnBeforeTuneEvent;


    private void CardTuner_OnAfterTuneEvent()
    {
      if (OnAfterTuneEvent != null)
      {
        OnAfterTuneEvent(_cardHandler);
      }
    }

    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="dbChannel">The db channel</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult CardTune(ref IUser user, IChannel channel, Channel dbChannel)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return TvResult.CardIsDisabled;
        }

        TvResult result;
        this.LogDebug("card: CardTune {0} {1} {2}:{3}:{4}", _cardHandler.DataBaseCard.CardId, channel.Name, user.Name,
                      user.CardId, _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, dbChannel.ChannelId));
        if (_cardHandler.IsScrambled(user.Name))
        {
          result = Tune(ref user, channel, dbChannel.ChannelId);
          this.LogInfo("card2:{0} {1} {2}", user.Name, user.CardId, _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, dbChannel.ChannelId));
          return result;
        }

        if (_cardHandler.CurrentDbChannel(user.Name) == dbChannel.ChannelId && dbChannel.ChannelId >= 0)
        {
          return TvResult.Succeeded;
        }
        result = Tune(ref user, channel, dbChannel.ChannelId);
        this.LogInfo("card2:{0} {1} {2}", user.Name, user.CardId, _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, dbChannel.ChannelId));
        return result;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        return TvResult.UnknownError;
      }
    }

    /// <summary>
    /// Determines whether card is tuned to the transponder specified by transponder
    /// </summary>
    /// <param name="transponder">The transponder.</param>
    /// <returns>
    /// 	<c>true</c> if card is tuned to the transponder; otherwise, <c>false</c>.
    /// </returns>
    public bool IsTunedToTransponder(IChannel transponder)
    {
      IChannel cardCurrentTuningDetail = _cardHandler.Card.CurrentTuningDetail;
      return (cardCurrentTuningDetail != null && !cardCurrentTuningDetail.IsDifferentTransponder(transponder));
    }


    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="channel">channel.</param>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return false;
        }       
        return _cardHandler.Card.CanTune(channel);
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        return false;
      }
    }
  }
}