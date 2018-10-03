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
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVControl
{
  /// <summary>
  /// Virtual Card class
  /// This class provides methods and properties which a client can use
  /// The class will handle the communication and control with the
  /// tv service backend
  /// </summary>
  [DataContract]
  public class VirtualCard : IVirtualCard
  {
    #region variables

    [DataMember]
    private int _nrOfOtherUsersTimeshiftingOnCard = 0;

    [DataMember]
    private IUser _user;

    [DataMember]
    private bool _isTimeshifting;

    [DataMember]
    private bool _isScanning;

    [DataMember]
    private bool _isRecording;

    [DataMember]
    private bool _isGrabbingEpg;

    [DataMember]
    private string _rtspUrl;

    [DataMember]
    private string _recordingFileName;

    [DataMember]
    private string _name;

    [DataMember]
    private BroadcastStandard _supportedBroadcastStandards = BroadcastStandard.Unknown;

    [DataMember]
    private string _timeShiftFileName;

    [DataMember]
    private string _channelName;    

    [DataMember]
    private int _idChannel = -1;

    [DataMember]
    private MediaType? _mediaType;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualCard"/> class.
    /// </summary>
    /// <param name="user">The user.</param>
    public VirtualCard(IUser user)
    {
      _user = user;

      InitStaticProperties();
    }

    private void InitStaticProperties()
    {
      string userName = _user.Name;
      int cardId = _user.CardId;

      var controllerService = GlobalServiceProvider.Get<IControllerService>();
      if (!string.IsNullOrWhiteSpace(userName))
      {
        _isTimeshifting = controllerService.IsTimeShifting(userName);
        _rtspUrl = controllerService.GetStreamingUrl(userName);
        _recordingFileName = controllerService.RecordingFileName(userName);
        _idChannel = controllerService.CurrentDbChannel(userName);
        _channelName = controllerService.CurrentChannelName(userName);

        if (_idChannel > 0)
        {
          //TODO
          //IChannel channel = controllerService.CurrentChannel(userName, _idChannel);
          //if (channel != null)
          //{
          //  _mediaType = channel.MediaType;
          //}
        }
        if (cardId > 0 && _user.UserType == UserType.Normal)
        {
          _timeShiftFileName = controllerService.TimeShiftFileName(userName, cardId);
        }
      }

      if (cardId > 0)
      {
        if (_idChannel > 0)
        {
          _isRecording = controllerService.IsRecording(_idChannel, cardId);
        }

        _isScanning = controllerService.IsScanning(cardId);
        _isGrabbingEpg = controllerService.IsGrabbingEpg(cardId);
        _name = controllerService.CardName(cardId);
        _supportedBroadcastStandards = controllerService.PossibleBroadcastStandards(cardId);
      }
    }

    #endregion
    
    #region properties

    #region static properties

    public MediaType? MediaType
    {
      get { return _mediaType; }
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <value>The user.</value>
    public IUser User
    {
      get { return _user; }
    }

    /// <summary>
    /// returns the card id of this virtual card
    /// </summary>
    public int Id
    {
      get { return _user.CardId; }
    }

    /// <summary>
    /// Get the broadcast standards supported by the tuner.
    /// </summary>
    public BroadcastStandard SupportedBroadcastStandards
    {
      get
      {
        return _supportedBroadcastStandards;
      }
    }

    /// <summary>
    /// Gets the name 
    /// </summary>
    /// <returns>name of card</returns>    
    public string Name
    {
      get
      {
        return _name;
        /*try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().CardName(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
      set { _name = value; }
    }


    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <returns>filename of the recording or null when not recording</returns>    
    public string RecordingFileName
    {
      get
      {
        return _recordingFileName;
        /*try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().RecordingFileName(_user.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
    }

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream 
    /// </summary>
    /// <returns>URL containing the RTSP adress on which the card transmits its stream</returns>

    public string RTSPUrl
    {
      get
      {
        return _rtspUrl;
        /*
        try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().GetStreamingUrl(User.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
    }

    /// <summary>
    /// Returns if we arecurrently grabbing the epg or not
    /// </summary>
    /// <returns>true when card is grabbing the epg otherwise false</returns>    
    public bool IsGrabbingEpg
    {
      get
      {
        return _isGrabbingEpg;
        /*try
        {
          if (User.CardId < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().IsGrabbingEpg(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _isGrabbingEpg = value; }
    }

    /// <summary>
    /// Returns if card is currently recording or not
    /// </summary>
    /// <returns>true when card is recording otherwise false</returns>    
    public bool IsRecording
    {
      get
      {
        return _isRecording;
        /*try
        {
          //if (User.CardId < 0) return false;
          RemoteControl.HostName = _server;          
          IVirtualCard vc = null;
          bool isRec = WaitFor<bool>.Run(CommandTimeOut, () => GlobalServiceProvider.Get<IControllerService>().IsRecording(IdChannel, out vc));
          return (isRec && vc.Id == Id && vc.User.UserType == UserType.Scheduler);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _isRecording = value; }
    }

    /// <summary>
    /// Returns if card is currently scanning or not
    /// </summary>
    /// <returns>true when card is scanning otherwise false</returns>    
    public bool IsScanning
    {
      get
      {
        return _isScanning;
        /*try
        {
          if (User.CardId < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().IsScanning(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _isScanning = value; }
    }

    /// <summary>
    /// Returns if card is currently timeshifting or not
    /// </summary>
    /// <returns>true when card is timeshifting otherwise false</returns>    
    public bool IsTimeShifting
    {
      get
      {
        return _isTimeshifting;
        /*try
        {
          if (User.CardId < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return WaitFor<bool>.Run(CommandTimeOut, () => GlobalServiceProvider.Get<IControllerService>().IsTimeShifting(_user.Name));
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _isTimeshifting = value; }
    }

    /// <summary>
    /// Returns the current filename used for timeshifting
    /// </summary>
    /// <returns>timeshifting filename null when not timeshifting</returns>    
    public string TimeShiftFileName
    {
      get
      {
        return _timeShiftFileName;
        /*try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().TimeShiftFileName(User.Name, User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
      set { _timeShiftFileName = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public int NrOfOtherUsersTimeshiftingOnCard
    {
      get { return _nrOfOtherUsersTimeshiftingOnCard; }
      set { _nrOfOtherUsersTimeshiftingOnCard = value; }
    }

    #endregion

    #region dynamic properties

    /// <summary>
    /// returns which schedule is currently being recorded
    /// </summary>
    /// <returns>id of Schedule or -1 if  card not recording</returns>
    [XmlIgnore]
    public int RecordingScheduleId
    {
      get
      {
        try
        {
          if (User.CardId < 0)
          {
            return -1;
          }
          return GlobalServiceProvider.Get<IControllerService>().GetRecordingSchedule(User.CardId, User.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return -1;
      }
    }

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="forceUpdate"><c>True</c> to force the signal status to be updated, and not use cached information.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    public void GetSignalStatus(bool forceUpdate, out bool isLocked, out bool isPresent, out int strength, out int quality)
    {
      isLocked = false;
      isPresent = false;
      strength = 0;
      quality = 0;
      try
      {
        if (User.CardId > 0)
        {
          GlobalServiceProvider.Get<IControllerService>().GetSignalStatus(User.CardId, forceUpdate, out isLocked, out isPresent, out strength, out quality);
        }
      }
      catch (Exception)
      {
        //HandleFailure();
      }
    }

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>
    /// <param name="totalTSpackets">Amount of packets processed</param>    
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>    
    public void GetStreamQualityCounters(out int totalTSpackets, out int discontinuityCounter)
    {
      totalTSpackets = 0;
      discontinuityCounter = 0;
      try
      {
        if (User.CardId > 0)
        {
          GlobalServiceProvider.Get<IControllerService>().GetStreamQualityCounters(User.Name, out totalTSpackets, out discontinuityCounter);
        }
      }
      catch (Exception)
      {
        //HandleFailure();
      }
    }

    /// <summary>
    /// Gets the name of the tv/radio channel to which we are tuned
    /// </summary>
    /// <returns>channel name</returns>    
    public string ChannelName
    {
      get
      {
        return _channelName;
        /*
        try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().CurrentChannelName(_user.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
    }

    /// <summary>
    /// returns the database channel
    /// </summary>
    /// <returns>int</returns>    
    public int IdChannel
    {
      get
      {
        return _idChannel;
        /*
        try
        {
          if (User.CardId < 0)
          {
            return -1;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().CurrentDbChannel(_user.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return -1;*/
      }
    }

    #endregion

    #region methods

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    public void StopTimeShifting()
    {
      try
      {
        if (User.CardId < 0)
        {
          return;
        }
        if (IsTimeShifting == false)
        {
          return;
        }
        IUser userResult;
        GlobalServiceProvider.Get<IControllerService>().StopTimeShifting(_user.Name, out userResult);

        if (userResult != null)
        {
          _user = userResult;          
        }
        _isTimeshifting = false;
        _timeShiftFileName = null;
        _rtspUrl = null;
        _name = null;
        _supportedBroadcastStandards = BroadcastStandard.AnalogTelevision;
      }
      catch (Exception)
      {
        //HandleFailure();
      }
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    public void StopRecording()
    {
      try
      {
        if (User.CardId < 0)
        {
          return;
        }
        IUser userResult;
        GlobalServiceProvider.Get<IControllerService>().StopRecording(_user.Name, _user.CardId, out userResult);
        if (userResult != null)
        {
          _user = userResult;
        }
      }
      catch (Exception)
      {
        //HandleFailure();
      }
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns>true if success otherwise false</returns>
    public TvResult StartRecording(ref string fileName)
    {
      try
      {
        if (User.CardId < 0)
        {
          return TvResult.UnknownError;
        }
        IUser userResult;
        TvResult startRecording = GlobalServiceProvider.Get<IControllerService>().StartRecording(_user.Name, _user.CardId, out userResult, ref fileName);

        if (userResult != null)
        {
          _user = userResult;
        }

        return startRecording;
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return TvResult.UnknownError;
    }

    #endregion

    #region quality control

    /// <summary>
    /// Indicates, if the user is the owner of the card
    /// </summary>
    /// <returns>true/false</returns>
    public bool IsOwner()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().IsOwner(User.CardId, User.Name);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports quality control
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsQualityControl()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }

        if (GlobalServiceProvider.Get<IControllerService>().GetSupportedQualityControlFeatures(User.CardId,
          out EncodeMode supportedEncodeModes, out bool canSetBitRate))
          return supportedEncodeModes != EncodeMode.Default;
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports bit rates
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsBitRate()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }
        if (GlobalServiceProvider.Get<IControllerService>().GetSupportedQualityControlFeatures(User.CardId,
          out EncodeMode supportedEncodeModes, out bool canSetBitRate))
          return canSetBitRate;
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports bit rate modes 
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsBitRateModes()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }
        if (GlobalServiceProvider.Get<IControllerService>().GetSupportedQualityControlFeatures(User.CardId,
          out EncodeMode supportedEncodeModes, out bool canSetBitRate))
          return supportedEncodeModes != EncodeMode.Default;
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports bit rate peak mode
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsPeakBitRateMode()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }
        if (GlobalServiceProvider.Get<IControllerService>().GetSupportedQualityControlFeatures(User.CardId,
          out EncodeMode supportedEncodeModes, out bool canSetBitRate))
          return supportedEncodeModes.HasFlag(EncodeMode.VariablePeakBitRate);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Gets/Sts the quality type
    /// </summary>
    public QualityType QualityType
    {
      get
      {
        try
        {
          if (User.CardId < 0)
          {
            return QualityType.Default;
          }
          // TODO
          //return GlobalServiceProvider.Get<IControllerService>().GetQualityType(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return QualityType.Default;
      }
      set
      {
        try
        {
          if (User.CardId < 0)
          {
            return;
          }
          // TODO
          // GlobalServiceProvider.Get<IControllerService>().SetQualityType(User.CardId, value);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
      }
    }

    /// <summary>
    /// Gets/Sts the bitrate mode
    /// </summary>
    public EncodeMode BitRateMode
    {
      get
      {
        try
        {
          if (User.CardId < 0)
          {
            return EncodeMode.Default;
          }
          return GlobalServiceProvider.Get<IControllerService>().GetEncodeMode(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return EncodeMode.Default;
      }
      set
      {
        try
        {
          if (User.CardId < 0)
          {
            return;
          }
          GlobalServiceProvider.Get<IControllerService>().SetEncodeMode(User.CardId, value);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
      }
    }

    #region CI Menu Handling

    /// <summary>
    /// Indicates, if the card supports CI Menu
    /// </summary>
    /// <returns>true/false</returns>
    public bool CiMenuSupported()
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().CiMenuSupported(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Enters the CI Menu for current card
    /// </summary>
    /// <returns>true if successful</returns>
    public bool EnterCiMenu()
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().EnterCiMenu(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Selects a ci menu entry
    /// </summary>
    /// <param name="Choice">Choice (1 based), 0 for "back"</param>
    /// <returns>true if successful</returns>
    public bool SelectCiMenu(byte Choice)
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().SelectMenu(User.CardId, Choice);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Closes the CI Menu for current card
    /// </summary>
    /// <returns>true if successful</returns>
    public bool CloseMenu()
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().CloseMenu(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Sends an answer to CAM after a request
    /// </summary>
    /// <param name="Cancel">cancel request</param>
    /// <param name="Answer">answer string</param>
    /// <returns>true if successful</returns>
    public bool SendMenuAnswer(bool Cancel, string Answer)
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().SendMenuAnswer(User.CardId, Cancel, Answer);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Sets a callback handler
    /// </summary>
    /// <param name="CallbackHandler"></param>
    /// <returns></returns>
    public bool SetCiMenuHandler(IConditionalAccessMenuCallBack CallbackHandler)
    {
      this.LogDebug("VC: SetCiMenuHandler");
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        this.LogDebug("VC: SetCiMenuHandler card: {0}, {1}", User.CardId, CallbackHandler);
        return GlobalServiceProvider.Get<IControllerService>().SetCiMenuHandler(User.CardId, CallbackHandler);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Exception");
        //HandleFailure();
      }
      return false;
    }

    #endregion

    #endregion    

    #endregion
  }
}