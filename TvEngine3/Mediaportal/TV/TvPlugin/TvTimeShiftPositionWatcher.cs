#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using MediaPortal.Configuration;
using MediaPortal.Player;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.TvPlugin
{

  internal class TvTimeShiftPositionWatcher
  {
    #region Variables

    private static int _idChannelToWatch = -1;
    private static long _snapshotBufferId = 0;
    private static long _bufferId = 0;
    private static Timer _timer = null;
    private static int _isEnabled = 0;
    public static bool needToCheck = false;
    private static Int64 _snapshotBufferPosition = -1;
    private static string _snapshotBufferFile = "";
    private static decimal _preRecordInterval = -1;
    private static int _secondsElapsed = 0;

    #endregion

    #region Event handlers

    static void _timer_Tick(object sender, EventArgs e)
    {
      CheckRecordingStatus();
      UpdateTimeShiftReusedStatus();
      _secondsElapsed++;

      if (_secondsElapsed == 60)
      {
        CheckOrUpdateTimeShiftPosition();
        _secondsElapsed = 0;
      }
    }

    #endregion

    #region Public methods

    public static void SetNewChannel(int idChannel)
    {
      if (!IsEnabled())
        return;

      _snapshotBufferPosition = -1;
      _snapshotBufferFile = "";
      _snapshotBufferId = 0;

      if (_preRecordInterval == -1)
      {
        _preRecordInterval = Decimal.Parse(ServiceAgents.Instance.SettingServiceAgent.GetValue("preRecordInterval", "5"));
      }
      Log.Debug("TvTimeShiftPositionWatcher: SetNewChannel(" + idChannel.ToString() + ")");
      _idChannelToWatch = idChannel;
      if (idChannel == -1)
      {
        _timer.Enabled = false;
        Log.Debug("TvTimeShiftPositionBuffer: Timer stopped because recording on this channel started or tv stopped.");
      }
      else
        StartTimer();
    }
    #endregion

    #region Private methods

    private static void StartTimer()
    {
      if (!IsEnabled())
        return;

      if (_timer == null)
      {
        _timer = new Timer();
        _timer.Interval = 500;
        _timer.Tick += new EventHandler(_timer_Tick);
      }
      Log.Debug("TvTimeShiftPositionWatcher: started");
      _timer.Enabled = true;
    }

    private static bool IsEnabled()
    {
      if (_isEnabled == 0)
      {
        if (DebugSettings.EnableRecordingFromTimeshift)
          _isEnabled = 1;
        else
          _isEnabled = -1;
      }
      return (_isEnabled == 1);
    }

    private static void CheckRecordingStatus()
    {
      try
      {
        if (TVHome.Card.IsRecording && needToCheck)
        {
          int scheduleId = TVHome.Card.RecordingScheduleId;
          if (scheduleId > 0)
          {
            Recording rec = ServiceAgents.Instance.RecordingServiceAgent.GetActiveRecording(scheduleId);
            Log.Info("TvTimeShiftPositionWatcher: Detected a started recording. ProgramName: {0}", rec.Title);
            InitiateBufferFilesCopyProcess(rec.FileName);
            needToCheck = false; // end of checking
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvTimeshiftPositionWatcher.CheckRecordingStatus exception : {0}", ex);
      }
    }

    private static void InitiateBufferFilesCopyProcess(string recordingFilename)
    {
      if (_snapshotBufferPosition == -1)
      {
        Log.Info("TvTimeshiftPositionWatcher: there is no program information, skip the ts buffer copy.");
        return;
      }

      IUser u = TVHome.Card.User;
      long bufferId = 0;
      Int64 currentPosition = -1;
      int maxFiles = 0;
      Int64 maximumFileSize = 0;
      ServiceAgents.Instance.ControllerServiceAgent.getTimeshiftParams(ref maxFiles, ref maximumFileSize);
      List<string[]> itemlist = new List<string[]>();

      if (ServiceAgents.Instance.ControllerServiceAgent.TimeShiftGetCurrentFilePosition(u.Name, ref currentPosition, ref bufferId))
      {
        string currentFile = ServiceAgents.Instance.ControllerServiceAgent.TimeShiftFileName(u.Name, u.CardId) + bufferId.ToString() + ".ts";

        Log.Info("TvTimeshiftPositionWatcher: current TS Position {0}, TS bufferId {1}, _snapshotBufferId {2}, recording file {0}",
          currentPosition, bufferId, _snapshotBufferId, recordingFilename);

        if (_snapshotBufferId < bufferId)
        {
          Log.Debug("TvTimeshiftPositionWatcher1: _snapshotBufferId {0}, bufferId {1}", _snapshotBufferId, bufferId);
          string nextFile;

          for (long i = _snapshotBufferId; i < bufferId; i++)
          {
            nextFile = ServiceAgents.Instance.ControllerServiceAgent.TimeShiftFileName(u.Name, u.CardId) + i + ".ts";
            Log.Debug("TvTimeshiftPositionWatcher2: nextFile {0}", nextFile);
            itemlist.Add(new[] { nextFile, string.Format("{0}", maximumFileSize), recordingFilename });
          }
        }
        else if (_snapshotBufferId > bufferId)
        {
          {
            string nextFile;

            for (long i = _snapshotBufferId; i <= maxFiles; i++)
            {
              nextFile = ServiceAgents.Instance.ControllerServiceAgent.TimeShiftFileName(u.Name, u.CardId) + i + ".ts";
              Log.Debug("TvTimeshiftPositionWatcher3: nextFile {0}", nextFile);
              itemlist.Add(new[] { nextFile, string.Format("{0}", maximumFileSize), recordingFilename });
            }

            if (1 < _bufferId)
            {
              for (long i = 1; i < _bufferId; i++)
              {
                nextFile = ServiceAgents.Instance.ControllerServiceAgent.TimeShiftFileName(u.Name, u.CardId) + i + ".ts";
                Log.Debug("TvTimeshiftPositionWatcher4: nextFile {0}", nextFile);
                itemlist.Add(new[] { nextFile, string.Format("{0}", maximumFileSize), recordingFilename });
              }
            }
          }
        }
        itemlist.Add(new[] { currentFile, string.Format("{0}", currentPosition), recordingFilename });
        Log.Debug("TvTimeshiftPositionWatcher: currentFile {0}", currentFile);
        ServiceAgents.Instance.ControllerServiceAgent.CopyTimeShiftFile(itemlist);
      }
      _snapshotBufferPosition = -1;
      _snapshotBufferFile = "";
      _snapshotBufferId = 0;
    }

    private static void SnapshotTimeShiftBuffer()
    {
      Log.Debug("TvTimeShiftPositionWatcher: Snapshotting timeshift buffer");
      IUser u = TVHome.Card.User;
      if (u == null)
      {
        Log.Error("TvTimeShiftPositionWatcher: Snapshot buffer failed. TvHome.Card.User==null");
        _snapshotBufferPosition = -1;
        _snapshotBufferId = 0;
        return;
      }

      if (!ServiceAgents.Instance.ControllerServiceAgent.TimeShiftGetCurrentFilePosition(u.Name, ref _snapshotBufferPosition, ref _snapshotBufferId))
      {
        Log.Error("TvTimeShiftPositionWatcher: TimeShiftGetCurrentFilePosition failed.");
        _snapshotBufferPosition = -1;
        _snapshotBufferId = 0;
        return;
      }
      _snapshotBufferFile = ServiceAgents.Instance.ControllerServiceAgent.TimeShiftFileName(u.Name, u.CardId) + _snapshotBufferId.ToString() + ".ts";
      Log.Info("TvTimeShiftPositionWatcher: Snapshot done - position: {0}, filename: {1}", _snapshotBufferPosition, _snapshotBufferFile);
    }

    private static void CheckOrUpdateTimeShiftPosition()
    {
      if (_idChannelToWatch == -1)
        return;
      if (!TVHome.Connected)
        return;
      ChannelBLL chan = new ChannelBLL(ServiceAgents.Instance.ChannelServiceAgent.GetChannel(_idChannelToWatch));
      if (chan.Entity == null)
        return;
      try
      {
        DateTime current = DateTime.Now;
        current = current.AddMinutes((double)_preRecordInterval);
        current = new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, 0);
        DateTime dtProgEnd = chan.CurrentProgram.EndTime;
        dtProgEnd = new DateTime(dtProgEnd.Year, dtProgEnd.Month, dtProgEnd.Day, dtProgEnd.Hour, dtProgEnd.Minute, 0);

        Log.Debug("TvTimeShiftPositionWatcher: Checking {0} == {1}, _bufferId {2}, _snapshotBufferId {3}",
          current.ToString("dd.MM.yy HH:mm"), dtProgEnd.ToString("dd.MM.yy HH:mm"), _bufferId, _snapshotBufferId);

        if (current == dtProgEnd)
        {
          Log.Debug("TvTimeShiftPositionWatcher: Next program starts within the configured Pre-Rec interval. Current program: [{0}] ending: {1}", chan.CurrentProgram.Title, chan.CurrentProgram.EndTime.ToString());
          SnapshotTimeShiftBuffer();
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvTimeshiftPositionWatcher.CheckOrUpdateTimeShiftPosition exception : {0}", ex);
      }
    }

    private static void UpdateTimeShiftReusedStatus()
    {
      Int64 currentPosition = -1;

      _bufferId = GetTimeShiftPosition(ref currentPosition);

      if (_snapshotBufferId == _bufferId && _snapshotBufferPosition > currentPosition)
      {
        _snapshotBufferPosition = -1;
        _snapshotBufferId = 0;
        Log.Info("TVHome: snapshot buffer Reused.");
      }

    }

    private static long GetTimeShiftPosition()
    {
      IUser u = TVHome.Card.User;
      long bufferId = 0;
      Int64 currentPosition = -1;
      if (ServiceAgents.Instance.ControllerServiceAgent.TimeShiftGetCurrentFilePosition(u.Name, ref currentPosition, ref bufferId))
      {
        return bufferId;
      }
      return 0;
    }

    private static long GetTimeShiftPosition(ref Int64 currentPosition)
    {
      IUser u = TVHome.Card.User;
      long bufferId = 0;
      if (ServiceAgents.Instance.ControllerServiceAgent.TimeShiftGetCurrentFilePosition(u.Name, ref currentPosition, ref bufferId))
      {
        return bufferId;
      }
      return 0;
    }
    #endregion
  }

  /*
  internal class TvTimeShiftPositionWatcher
  {
 

    #region Variables
    private static int idChannelToWatch = -1;
    private static Int64 snapshotBuferPosition = -1;
    private static string snapshotBufferFile = "";
    private static decimal preRecordInterval = -1;
    private static Timer _timer=null;
    private static int isEnabled = 0;
    private static int secondsElapsed = 0;
    #endregion

    #region Event handlers
    static void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type == g_Player.MediaType.TV)
      {
        SetNewChannel(-1);
      }
    }
    static void _timer_Tick(object sender, EventArgs e)
    {
      CheckRecordingStatus();
      secondsElapsed++;
      if (secondsElapsed == 60)
      {
        CheckOrUpdateTimeShiftPosition();
        secondsElapsed = 0;
      }
    }
    #endregion

    #region Private members
    private static bool IsEnabled()
    {
      if (isEnabled == 0)
      {
        if (DebugSettings.EnableRecordingFromTimeshift)
          isEnabled = 1;
        else
          isEnabled = -1;
      }
      return (isEnabled == 1);
    }
    private static void StartTimer()
    {
      if (_timer == null)
      {
        _timer = new Timer();
        _timer.Interval = 1000;
        _timer.Tick += new EventHandler(_timer_Tick);
        g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
      }
      Log.Debug("TvTimeShiftPositionWatcher: Channel changed.");
      SnapshotTimeShiftBuffer();
      secondsElapsed = 0;
      _timer.Enabled = true;
    }
    private static void SnapshotTimeShiftBuffer()
    {
      Log.Debug("TvTimeShiftPositionWatcher: Snapshotting timeshift buffer");
      IUser u = TVHome.Card.User;
      if (u == null)
      {
        Log.Error("TvTimeShiftPositionWatcher: Snapshot buffer failed. TvHome.Card.User==null");
        return;
      }
      long bufferId = 0;
      if (!ServiceAgents.Instance.ControllerServiceAgent.TimeShiftGetCurrentFilePosition(u.Name, ref snapshotBuferPosition, ref bufferId))
      {
        Log.Error("TvTimeShiftPositionWatcher: TimeShiftGetCurrentFilePosition failed.");
        return;
      }
      snapshotBufferFile = ServiceAgents.Instance.ControllerServiceAgent.TimeShiftFileName(u.Name, u.CardId) + bufferId.ToString() + ".ts";
      Log.Debug("TvTimeShiftPositionWatcher: Snapshot done - position: {0}, filename: {1}", snapshotBuferPosition, snapshotBufferFile);
    }
    private static void CheckRecordingStatus()
    {
      try
      {
        if (TVHome.Card.IsRecording)
        {
          int scheduleId = TVHome.Card.RecordingScheduleId;
          if (scheduleId > 0)
          {
            Recording rec = ServiceAgents.Instance.RecordingServiceAgent.GetActiveRecording(scheduleId);
            Log.Debug("TvTimeShiftPositionWatcher: Detected a started recording. ProgramName: {0}", rec.Title);
            InitiateBufferFilesCopyProcess(rec.FileName);
            SetNewChannel(-1);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvTimeshiftPositionWatcher.CheckRecordingStatus exception : {0}", ex);
      }
    }
    private static void CheckOrUpdateTimeShiftPosition()
    {
      if (idChannelToWatch == -1)
        return;
      if (!TVHome.Connected)
        return;
      ChannelBLL chan = new ChannelBLL(ServiceAgents.Instance.ChannelServiceAgent.GetChannel(idChannelToWatch));
      if (chan.Entity == null)
        return;
      try
      {
        DateTime current = DateTime.Now;
        current = current.AddMinutes((double)preRecordInterval);
        current = new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, 0);
        DateTime dtProgEnd = chan.CurrentProgram.EndTime;
        dtProgEnd = new DateTime(dtProgEnd.Year, dtProgEnd.Month, dtProgEnd.Day, dtProgEnd.Hour, dtProgEnd.Minute, 0);
        Log.Debug("TvTimeShiftPositionWatcher: Checking {0} == {1}", current.ToString("dd.MM.yy HH:mm"), dtProgEnd.ToString("dd.MM.yy HH:mm"));
        if (current == dtProgEnd)
        {
          Log.Debug("TvTimeShiftPositionWatcher: Next program starts within the configured Pre-Rec interval. Current program: [{0}] ending: {1}", chan.CurrentProgram.Title, chan.CurrentProgram.EndTime.ToString());
          SnapshotTimeShiftBuffer();
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvTimeshiftPositionWatcher.CheckOrUpdateTimeShiftPosition exception : {0}", ex);
      }
    }
    private static void InitiateBufferFilesCopyProcess(string recordingFilename)
    {
      if (!IsEnabled())
        return;
      if (snapshotBuferPosition != -1)
      {
        IUser u = TVHome.Card.User;
        long bufferId = 0;
        Int64 currentPosition = -1;
        if (ServiceAgents.Instance.ControllerServiceAgent.TimeShiftGetCurrentFilePosition(u.Name, ref currentPosition, ref bufferId))
        {
          string currentFile = ServiceAgents.Instance.ControllerServiceAgent.TimeShiftFileName(u.Name, u.CardId) + bufferId.ToString() + ".ts";
          Log.Info("**");
          Log.Info("**");
          Log.Info("**");
          Log.Info("TvTimeshiftPositionWatcher: Starting to copy buffer files for recording {0}", recordingFilename);
          Log.Info("**");
          Log.Info("**");
          Log.Info("**");
          ServiceAgents.Instance.ControllerServiceAgent.CopyTimeShiftFile(snapshotBuferPosition, snapshotBufferFile, currentPosition,
                                                   currentFile, recordingFilename);
        }
      }
    }
    #endregion

    public static void SetNewChannel(int idChannel)
    {
      if (!IsEnabled())
        return;
      if (preRecordInterval == -1)
      {
        preRecordInterval = Decimal.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("preRecordInterval", "5").Value);
      }
      Log.Debug("TvTimeShiftPositionWatcher: SetNewChannel(" + idChannel.ToString() + ")");
      idChannelToWatch = idChannel;
      if (idChannel == -1)
      {
        snapshotBuferPosition = -1;
        snapshotBufferFile = "";
        _timer.Enabled = false;
        Log.Debug("TvTimeShiftPositionBuffer: Timer stopped because recording on this channel started or tv stopped.");
      }
      else
        StartTimer();
    }
  }*/
}