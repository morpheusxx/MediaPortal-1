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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// A base class for all tuners, independent of tuner implementation and stream format.
  /// </summary>
  internal abstract class TunerBase : ITunerInternal, ITVCard, IDisposable
  {
    #region events

    /// <summary>
    /// New sub-channel delegate, invoked when a new sub-channel is created.
    /// </summary>
    private OnNewSubChannelDelegate _newSubChannelEventDelegate = null;

    /// <summary>
    /// Set the tuner's new sub-channel event handler.
    /// </summary>
    /// <value>the delegate</value>
    public event OnNewSubChannelDelegate OnNewSubChannelEvent
    {
      add
      {
        _newSubChannelEventDelegate = null;
        _newSubChannelEventDelegate += value;
      }
      remove
      {
        _newSubChannelEventDelegate = null;
      }
    }

    /// <summary>
    /// Fire the new sub-channel observer event.
    /// </summary>
    /// <param name="subChannelId">The ID of the new sub-channel.</param>
    private void FireNewSubChannelEvent(int subChannelId)
    {
      if (_newSubChannelEventDelegate != null)
      {
        _newSubChannelEventDelegate(subChannelId);
      }
    }

    /// <summary>
    /// After tune delegate, fired after tuning is complete.
    /// </summary>
    private OnAfterTuneDelegate _afterTuneEventDelegate;

    /// <summary>
    /// Set the tuner's after tune event handler.
    /// </summary>
    /// <value>the delegate</value>
    public event OnAfterTuneDelegate OnAfterTuneEvent
    {
      add
      {
        _afterTuneEventDelegate = null;
        _afterTuneEventDelegate += value;
      }
      remove
      {
        _afterTuneEventDelegate = null;
      }
    }

    /// <summary>
    /// Fire the after tune observer event.
    /// </summary>
    private void FireAfterTuneEvent()
    {
      if (_afterTuneEventDelegate != null)
      {
        _afterTuneEventDelegate();
      }
    }

    #endregion

    #region variables

    /// <summary>
    /// Dictionary of sub-channels.
    /// </summary>
    private Dictionary<int, ITvSubChannel> _mapSubChannels = new Dictionary<int, ITvSubChannel>();

    /// <summary>
    /// The ID to use for the next new sub-channel.
    /// </summary>
    private int _nextSubChannelId = 0;

    /// <summary>
    /// Context reference
    /// </summary>
    private object _context = null;

    #region identification

    /// <summary>
    /// The tuner's unique identifier.
    /// </summary>
    /// <remarks>
    /// This is the identifier for the database record which holds the tuner's
    /// settings.
    /// </remarks>
    private int _tunerId = -1;

    /// <summary>
    /// The tuner's unique external identifier.
    /// </summary>
    /// <remarks>
    /// The source of this identifier varies from implementation to
    /// implementation. For example, implementations based on DirectShow
    /// may use the IMoniker display name (AKA device path).
    /// </remarks>
    private readonly string _externalId = string.Empty;

    /// <summary>
    /// A shared identifier for all tuner instances derived from a
    /// [multi-tuner] product.
    /// </summary>
    protected string _productInstanceId = null;

    /// <summary>
    /// A shared identifier for all tuner instances derived from a single
    /// physical tuner.
    /// </summary>
    protected string _tunerInstanceId = null;

    /// <summary>
    /// The tuner's name.
    /// </summary>
    private string _name = string.Empty;

    /// <summary>
    /// The tuner type (eg. DVB-S, DVB-T... etc.).
    /// </summary>
    private readonly CardType _tunerType = CardType.Unknown;

    #endregion

    #region signal status

    /// <summary>
    /// Indicates if the tuner is locked onto signal.
    /// </summary>
    private volatile bool _isSignalLocked = false;

    /// <summary>
    /// Indicates if the tuner has detected signal.
    /// </summary>
    private bool _isSignalPresent = false;

    /// <summary>
    /// The signal strength. Range: 0 to 100.
    /// </summary>
    private int _signalStrength = 0;

    /// <summary>
    /// The signal quality. Range: 0 to 100.
    /// </summary>
    private int _signalQuality = 0;

    /// <summary>
    /// Date and time of the last signal status update.
    /// </summary>
    private DateTime _lastSignalStatusUpdate = DateTime.MinValue;

    #endregion

    /// <summary>
    /// The action that will be taken when the tuner is no longer being actively used.
    /// </summary>
    private IdleMode _idleMode = IdleMode.Stop;

    /// <summary>
    /// A list containing the extension interfaces supported by this tuner. The list is ordered by
    /// descending extension priority.
    /// </summary>
    private IList<ICustomDevice> _extensions = new List<ICustomDevice>();

    /// <summary>
    /// A list containing the conditional access provider extensions supported by this tuner. The
    /// list is ordered by descending extension priority.
    /// </summary>
    private List<IConditionalAccessProvider> _caProviders = new List<IConditionalAccessProvider>();

    /// <summary>
    /// Enable or disable the use of conditional access interface(s).
    /// </summary>
    private bool _useConditionalAccessInterface = true;

    /// <summary>
    /// The type of conditional access module available to the conditional access interface.
    /// </summary>
    /// <remarks>
    /// Certain conditional access modules require specific handling to ensure compatibility.
    /// </remarks>
    private CamType _camType = CamType.Default;

    /// <summary>
    /// The number of channels that the tuner is capable of or permitted to decrypt simultaneously.
    /// Zero means there is no limit.
    /// </summary>
    private int _decryptLimit = 0;

    /// <summary>
    /// The method that should be used to communicate the set of channels that the tuner's conditional access
    /// interface needs to manage.
    /// </summary>
    /// <remarks>
    /// Multi-channel decrypt is *not* the same as Digital Devices' multi-transponder decrypt (MTD). MCD is a
    /// implmented using standard CA PMT commands; MTD is implemented in the Digital Devices drivers.
    /// Disabled = Always send Only. In most cases this will result in only one channel being decrypted. If other
    ///         methods are not working reliably then this one should at least allow decrypting one channel
    ///         reliably.
    /// List = Explicit management using Only, First, More and Last. This is the most widely supported set
    ///         of commands, however they are not suitable for some interfaces (such as the Digital Devices
    ///         interface).
    /// Changes = Use Add, Update and Remove to pass changes to the interface. The full channel list is never
    ///         passed. Most interfaces don't support these commands.
    /// </remarks>
    private MultiChannelDecryptMode _multiChannelDecryptMode = MultiChannelDecryptMode.List;

    /// <summary>
    /// Enable or disable waiting for the conditional interface to be ready before sending commands.
    /// </summary>
    private bool _waitUntilCaInterfaceReady = true;

    /// <summary>
    /// The number of times to re-attempt decrypting the current service set when one or more services are
    /// not able to be decrypted for whatever reason.
    /// </summary>
    /// <remarks>
    /// Each available CA interface will be tried in order of priority. If decrypting is not started
    /// successfully, all interfaces are retried until each interface has been tried
    /// _decryptFailureRetryCount + 1 times, or until decrypting is successful.
    /// </remarks>
    private int _decryptFailureRetryCount = 2;

    /// <summary>
    /// The tuner's current tuning parameter values or null if the tuner is not tuned.
    /// </summary>
    private IChannel _currentTuningDetail = null;

    /// <summary>
    /// Enable or disable the use of extensions for tuning.
    /// </summary>
    /// <remarks>
    /// Custom/direct tuning *may* be faster or more reliable than regular tuning methods. It might
    /// be slower (eg. TeVii) or more limiting (eg. Digital Everywhere) than regular tuning
    /// methods. User gets to choose which method to use.
    /// </remarks>
    private bool _useCustomTuning = false;

    /// <summary>
    /// A flag used by the TV service as a signal to abort the tuning process before it is completed.
    /// </summary>
    private volatile bool _cancelTune = false;

    /// <summary>
    /// The current state of the tuner.
    /// </summary>
    private TunerState _state = TunerState.NotLoaded;

    /// <summary>
    /// Does the tuner support receiving more than one service simultaneously?
    /// </summary>
    /// <remarks>
    /// This may seem obvious and unnecessary, especially for modern tuners. However even today
    /// there are tuners that cannot receive more than one service simultaneously. CableCARD tuners
    /// are a good example.
    /// </remarks>
    protected bool _supportsSubChannels = true;

    /// <summary>
    /// The tuner group that this tuner is a member of, if any.
    /// </summary>
    private ITunerGroup _group = null;

    /// <summary>
    /// The tuner's DiSEqC control interface.
    /// </summary>
    private IDiseqcController _diseqcController = null;

    /// <summary>
    /// The tuner's encoder control interface.
    /// </summary>
    private IQuality _encoderController = null;

    /// <summary>
    /// The maximum length of time to wait for signal detection after tuning.
    /// </summary>
    private int _timeOutWaitForSignal = 2000;   // milliseconds

    #endregion

    #region constructor

    /// <summary>
    /// Base constructor
    /// </summary>
    /// <param name="name">The name for the tuner.</param>
    /// <param name="externalId">The unique external identifier for the tuner.</param>
    /// <param name="type">The tuner type.</param>
    protected TunerBase(string name, string externalId, CardType type)
    {
      _name = name;
      _externalId = externalId;
      _tunerType = type;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public virtual void Dispose()
    {
      Unload();
    }

    #endregion

    #region properties

    /// <summary>
    /// Get the tuner's unique identifier.
    /// </summary>
    public int TunerId
    {
      get
      {
        return _tunerId;
      }
    }

    /// <summary>
    /// Get or set the tuner's group.
    /// </summary>
    public ITunerGroup Group
    {
      get
      {
        return _group;
      }
      set
      {
        _group = value;
      }
    }

    /// <summary>
    /// Get the tuner's name.
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
    }

    /// <summary>
    /// Get the tuner's unique external identifier.
    /// </summary>
    public string ExternalId
    {
      get
      {
        return _externalId;
      }
    }

    /// <summary>
    /// Get the tuner's product instance identifier.
    /// </summary>
    public string ProductInstanceId
    {
      get
      {
        return _productInstanceId;
      }
    }

    /// <summary>
    /// Get the tuner's instance identifier.
    /// </summary>
    public string TunerInstanceId
    {
      get
      {
        return _tunerInstanceId;
      }
    }

    /// <summary>
    /// Get the tuner's type.
    /// </summary>
    public CardType TunerType
    {
      get
      {
        return _tunerType;
      }
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _name;
    }

    #region conditional access properties

    /// <summary>
    /// Get/set the type of conditional access module available to the conditional access interface.
    /// </summary>
    /// <value>The type of the cam.</value>
    public CamType CamType
    {
      get
      {
        return _camType;
      }
    }

    /// <summary>
    /// Get the tuner's conditional access interface decrypt limit. This is usually the number of channels
    /// that the interface is able to decrypt simultaneously. A value of zero indicates that the limit is
    /// to be ignored.
    /// </summary>
    public int DecryptLimit
    {
      get
      {
        return _decryptLimit;
      }
    }

    /// <summary>
    /// Does the tuner support conditional access?
    /// </summary>
    /// <value><c>true</c> if the tuner supports conditional access, otherwise <c>false</c></value>
    public bool IsConditionalAccessSupported
    {
      get
      {
        if (!_useConditionalAccessInterface)
        {
          return false;
        }
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return _caProviders.Count > 0;
      }
    }

    /// <summary>
    /// Get the tuner's conditional access menu interaction interface.
    /// </summary>
    /// <value><c>null</c> if the tuner does not support conditional access</value>
    public IConditionalAccessMenuActions CaMenuInterface
    {
      get
      {
        if (!_useConditionalAccessInterface)
        {
          return null;
        }
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }

        // Return the first extension that implements CA menu access.
        foreach (ICustomDevice extension in _extensions)
        {
          IConditionalAccessMenuActions caMenuInterface = extension as IConditionalAccessMenuActions;
          if (caMenuInterface != null)
          {
            return caMenuInterface;
          }
        }
        return null;
      }
    }

    /// <summary>
    /// Get a count of the number of services that the tuner is currently decrypting.
    /// </summary>
    /// <value>The number of services currently being decrypted.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {
        // If not decrypting any channels or the limit is diabled then return zero.
        if (_mapSubChannels == null || _mapSubChannels.Count == 0 || _decryptLimit == 0)
        {
          return 0;
        }

        HashSet<long> decryptedServices = new HashSet<long>();
        Dictionary<int, ITvSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          IChannel service = en.Current.Value.CurrentChannel;
          if (!service.FreeToAir)
          {
            DVBBaseChannel digitalService = service as DVBBaseChannel;
            if (digitalService != null)
            {
              decryptedServices.Add(digitalService.ServiceId);
            }
            else
            {
              AnalogChannel analogService = service as AnalogChannel;
              if (analogService != null)
              {
                decryptedServices.Add(analogService.Frequency);
              }
              else
              {
                throw new TvException("tuner base: service type not recognised, unable to count number of services being decrypted\r\n" + service.ToString());
              }
            }
          }
        }

        return decryptedServices.Count;
      }
    }

    #endregion

    /// <summary>
    /// Get the tuner's DiSEqC control interface.
    /// </summary>
    /// <value><c>null</c> if the tuner is not a satellite tuner or the tuner
    /// does not support sending/receiving DiSEqC commands</value>
    public IDiseqcController DiseqcController
    {
      get
      {
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return _diseqcController;
      }
    }

    /// <summary>
    /// Get an indicator to determine whether the tuner is locked on signal.
    /// </summary>
    public bool IsTunerLocked
    {
      get
      {
        UpdateSignalStatus();
        return _isSignalLocked;
      }
    }

    /// <summary>
    /// Get the tuner input signal quality.
    /// </summary>
    public int SignalQuality
    {
      get
      {
        UpdateSignalStatus();
        return _signalQuality;
      }
    }

    /// <summary>
    /// Get the tuner input signal level.
    /// </summary>
    public int SignalLevel
    {
      get
      {
        UpdateSignalStatus();
        return _signalStrength;
      }
    }

    /// <summary>
    /// Reset the signal update timer.
    /// </summary>
    /// <remarks>
    /// Calling this function will force us to update signal
    /// information (rather than return cached values) next time
    /// a signal information query is received.
    /// </remarks>
    public void ResetSignalUpdate()
    {
      _lastSignalStatusUpdate = DateTime.MinValue;
    }

    /// <summary>
    /// Gets or sets the context.
    /// </summary>
    /// <value>The context.</value>
    public object Context
    {
      get { return _context; }
      set { _context = value; }
    }

    /// <summary>
    /// Get the tuning parameters that have been applied to the hardware.
    /// This property returns null when the tuner is not in use.
    /// </summary>
    public IChannel CurrentTuningDetail
    {
      get
      {
        return _currentTuningDetail;
      }
    }

    #endregion

    /// <summary>
    /// Open any <see cref="T:TvLibrary.Interfaces.ICustomDevice"/> extensions loaded for this tuner.
    /// </summary>
    /// <remarks>
    /// We separate this from the loading because some extensions (for example, the NetUP extension)
    /// can't be opened until the graph has finished being built.
    /// </remarks>
    private void OpenExtensions()
    {
      this.LogDebug("tuner base: open tuner extensions");

      foreach (ICustomDevice extension in _extensions)
      {
        if (_useConditionalAccessInterface)
        {
          IConditionalAccessProvider caProvider = extension as IConditionalAccessProvider;
          if (caProvider != null)
          {
            this.LogDebug("tuner base: found conditional access provider \"{0}\"", extension.Name);
            if (caProvider.OpenConditionalAccessInterface())
            {
              _caProviders.Add(caProvider);
            }
            else
            {
              this.LogDebug("tuner base: provider will not be used");
            }
          }
        }
        if (_diseqcController == null)
        {
          IDiseqcDevice diseqcDevice = extension as IDiseqcDevice;
          if (diseqcDevice != null)
          {
            this.LogDebug("tuner base: found DiSEqC control interface \"{0}\"", extension.Name);
            _diseqcController = new DiseqcController(TunerId, diseqcDevice);
          }
        }
        if (_encoderController == null)
        {
          IEncoder encoder = extension as IEncoder;
          if (encoder != null)
          {
            this.LogDebug("tuner base: found encoder control interface \"{0}\"", extension.Name);
            _encoderController = new EncoderController(_extensions);
          }
        }
        IRemoteControlListener rcListener = extension as IRemoteControlListener;
        if (rcListener != null)
        {
          this.LogDebug("tuner base: found remote control interface \"{0}\"", extension.Name);
          if (!rcListener.OpenRemoteControlInterface())
          {
            this.LogDebug("tuner base: interface will not be used");
          }
        }
      }
    }

    #region abstract and virtual methods

    /// <summary>
    /// Load the tuner.
    /// </summary>
    private void Load()
    {
      if (_state == TunerState.Loading)
      {
        this.LogWarn("tuner base: the tuner is already loading");
        return;
      }
      if (_state != TunerState.NotLoaded)
      {
        this.LogWarn("tuner base: the tuner is already loaded");
        return;
      }
      _state = TunerState.Loading;

      // Related tuners must be unloaded before this tuner can be loaded.
      if (_group != null)
      {
        this.LogDebug("tuner base: unload tuners in group");
        foreach (ITVCard tuner in _group.Tuners)
        {
          if (tuner.TunerId != TunerId)
          {
            TunerBase tunerBase = tuner as TunerBase;
            if (tunerBase != null)
            {
              tunerBase.Unload();
            }
          }
        }
      }

      this.LogDebug("tuner base: load tuner {0}", _name);
      try
      {
        ReloadConfiguration();
        _extensions = PerformLoading();

        _state = TunerState.Stopped;

        // Open any extensions that were detected during loading. This is
        // separated from loading because some extensions can't be opened until
        // the tuner has fully loaded.
        OpenExtensions();

        // Extensions can request to pause or start the tuner - other actions
        // don't make sense here. The started state is considered more
        // compatible than the paused state, so start takes precedence.
        TunerAction actualAction = TunerAction.Default;
        foreach (ICustomDevice extension in _extensions)
        {
          TunerAction action;
          extension.OnLoaded(this, out action);
          if (action == TunerAction.Pause)
          {
            if (actualAction == TunerAction.Default)
            {
              this.LogDebug("tuner base: extension \"{0}\" will cause tuner pause", extension.Name);
              actualAction = TunerAction.Pause;
            }
            else
            {
              this.LogDebug("tuner base: extension \"{0}\" wants to pause the tuner, overriden", extension.Name);
            }
          }
          else if (action == TunerAction.Start)
          {
            this.LogDebug("tuner base: extension \"{0}\" will cause tuner start", extension.Name);
            actualAction = action;
          }
          else if (action != TunerAction.Default)
          {
            this.LogDebug("tuner base: extension \"{0}\" wants unsupported action {1}", extension.Name, action);
          }
        }

        if (actualAction == TunerAction.Default && _idleMode == IdleMode.AlwaysOn)
        {
          this.LogDebug("tuner base: tuner is configured as always on");
          actualAction = TunerAction.Start;
        }

        if (actualAction != TunerAction.Default)
        {
          PerformTunerAction(actualAction);
        }
      }
      catch (TvExceptionSWEncoderMissing)
      {
        Unload();
        throw;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuner base: failed to load tuner");
        Unload();
        throw new TvExceptionTunerLoadFailed("Failed to load tuner.", ex);
      }
    }

    /// <summary>
    /// Unload the tuner.
    /// </summary>
    private void Unload()
    {
      this.LogDebug("tuner base: unload tuner");
      FreeAllSubChannels();
      try
      {
        PerformTunerAction(TunerAction.Stop);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuner base: failed to stop tuner before unloading");
      }

      // Dispose extensions.
      if (_extensions != null)
      {
        foreach (ICustomDevice extension in _extensions)
        {
          // Avoid recursive loop for ITVCard implementations that also implement ICustomDevice.
          if (!(extension is ITVCard))
          {
            extension.Dispose();
          }
        }
      }
      _caProviders.Clear();
      _extensions.Clear();

      try
      {
        PerformUnloading();
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "tuner base: failed to completely unload the tuner");
      }

      _diseqcController = null;
      _encoderController = null;

      _state = TunerState.NotLoaded;
    }

    /// <summary>
    /// Wait for the tuner to acquire signal lock.
    /// </summary>
    private void LockInOnSignal()
    {
      this.LogDebug("tuner base: lock in on signal");
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      bool isLocked;
      while (ts.TotalMilliseconds < _timeOutWaitForSignal)
      {
        ThrowExceptionIfTuneCancelled();
        GetSignalStatus(true, out isLocked, out _isSignalPresent, out _signalStrength, out _signalQuality);
        _isSignalLocked = isLocked;
        if (isLocked)
        {
          this.LogDebug("tuner base: locked");
          return;
        }
        ts = DateTime.Now - timeStart;
        System.Threading.Thread.Sleep(20);
      }

      throw new TvExceptionNoSignal("Failed to lock signal.");
    }

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public virtual void ReloadConfiguration()
    {
      this.LogDebug("tuner base: reload configuration");
      if (ExternalId != null)
      {
        Card t = CardManagement.GetCardByDevicePath(ExternalId, CardIncludeRelationEnum.None);
        if (t != null)
        {
          _tunerId = t.CardId;
          _name = t.Name;   // We prefer to use the name that can be set via configuration for more readable logs...
          _idleMode = (IdleMode)t.IdleMode;
          _pidFilterMode = (PidFilterMode)t.PidFilterMode;
          _useCustomTuning = t.UseCustomTuning;

          // Conditional access...
          _useConditionalAccessInterface = t.UseConditionalAccess;
          _camType = (CamType)t.CamType;
          _decryptLimit = t.DecryptLimit;
          _multiChannelDecryptMode = (MultiChannelDecryptMode)t.MultiChannelDecryptMode;

          if (_state == TunerState.NotLoaded && t.PreloadCard)
          {
            Load();
          }
        }
      }

      _timeOutWaitForSignal = SettingsManagement.GetValue("timeoutTune", 2) * 1000;

      foreach (ITvSubChannel subChannel in _mapSubChannels.Values)
      {
        subChannel.ReloadConfiguration();
      }

      if (InternalEpgGrabberInterface != null)
      {
        InternalEpgGrabberInterface.ReloadConfiguration();
      }
      if (_diseqcController != null)
      {
        _diseqcController.ReloadConfiguration();
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public IEpgGrabber EpgGrabberInterface
    {
      get
      {
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return InternalEpgGrabberInterface;
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public virtual IEpgGrabber InternalEpgGrabberInterface
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public IChannelScanner ChannelScanningInterface
    {
      get
      {
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return InternalChannelScanningInterface;
      }
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public abstract IChannelScannerInternal InternalChannelScanningInterface
    {
      get;
    }

    /// <summary>
    /// Update tuner signal status measurements.
    /// </summary>
    private void UpdateSignalStatus()
    {
      TimeSpan ts = DateTime.Now - _lastSignalStatusUpdate;
      if (ts.TotalMilliseconds < 5000)
      {
        return;
      }
      if (_currentTuningDetail == null || _state != TunerState.Started)
      {
        _isSignalLocked = false;
        _isSignalPresent = false;
        _signalStrength = 0;
        _signalQuality = 0;
      }
      else
      {
        bool isLocked;
        GetSignalStatus(false, out isLocked, out _isSignalPresent, out _signalStrength, out _signalQuality);
        _isSignalLocked = isLocked;
        _lastSignalStatusUpdate = DateTime.Now;
      }
    }

    /// <summary>
    /// Stop the tuner.
    /// </summary>
    /// <remarks>
    /// The actual result of this function depends on tuner configuration.
    /// </remarks>
    public void Stop()
    {
      this.LogDebug("tuner base: stop, idle mode = {0}", _idleMode);
      TunerAction action = TunerAction.Stop;
      try
      {
        if (InternalEpgGrabberInterface != null && InternalEpgGrabberInterface.IsEpgGrabbing)
        {
          InternalEpgGrabberInterface.AbortGrabbing();
        }
        if (InternalChannelScanningInterface != null && InternalChannelScanningInterface.IsScanning)
        {
          InternalChannelScanningInterface.AbortScanning();
        }
        FreeAllSubChannels();

        switch (_idleMode)
        {
          case IdleMode.Pause:
            action = TunerAction.Pause;
            break;
          case IdleMode.Unload:
            action = TunerAction.Unload;
            break;
          case IdleMode.AlwaysOn:
            action = TunerAction.Start;
            break;
        }

        // Extensions may want to prevent or direct actions to ensure
        // compatibility and smooth tuner operation.
        TunerAction extensionAction = action;
        foreach (ICustomDevice extension in _extensions)
        {
          extension.OnStop(this, ref extensionAction);
          if (extensionAction > action)
          {
            this.LogDebug("tuner base: extension \"{0}\" overrides action {1} with {2}", extension.Name, action, extensionAction);
            action = extensionAction;
          }
          else if (action != extensionAction)
          {
            this.LogWarn("tuner base: extension \"{0}\" wants to perform action {1}, overriden", extension.Name, extensionAction);
          }
        }

        try
        {
          PerformTunerAction(action);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "tuner base: failed to stop tuner with action {0}", action);
        }

        // Turn off the tuner power.
        foreach (ICustomDevice extension in _extensions)
        {
          IPowerDevice powerDevice = extension as IPowerDevice;
          if (powerDevice != null)
          {
            powerDevice.SetPowerState(PowerState.Off);
          }
        }
      }
      finally
      {
        if (action != TunerAction.Start && action != TunerAction.Pause)
        {
          // This forces a full retune. There are potential tuner compatibility
          // considerations here. Most tuners should remember current settings
          // when paused; some do when stopped as well, but the majority don't.
          _currentTuningDetail = null;
          if (_diseqcController != null)
          {
            _diseqcController.SwitchToChannel(null);
          }
        }
      }
    }

    /// <summary>
    /// Perform a specific tuner action. For example, stop the tuner.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    private void PerformTunerAction(TunerAction action)
    {
      // Don't do anything if the tuner is not loaded.
      if (_state == TunerState.NotLoaded)
      {
        return;
      }
      this.LogDebug("tuner base: perform tuner action, action = {0}", action);

      if (action == TunerAction.Reset)
      {
        Unload();
        Load();
      }
      else if (action == TunerAction.Unload)
      {
        Unload();
      }
      else if (action == TunerAction.Pause)
      {
        SetTunerState(TunerState.Paused);
      }
      else if (action == TunerAction.Stop)
      {
        SetTunerState(TunerState.Stopped);
      }
      else if (action == TunerAction.Start)
      {
        SetTunerState(TunerState.Started);
      }
      else if (action == TunerAction.Restart)
      {
        SetTunerState(TunerState.Stopped);
        SetTunerState(TunerState.Started);
      }
      else
      {
        this.LogWarn("tuner base: unhandled action {0}", action);
        return;
      }

      this.LogDebug("tuner base: action succeeded");
    }

    /// <summary>
    /// Set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    private void SetTunerState(TunerState state)
    {
      this.LogDebug("tuner base: set tuner state, current state = {0}, requested state = {1}", _state, state);

      if (state == _state)
      {
        this.LogDebug("tuner base: tuner already in required state");
        return;
      }

      PerformSetTunerState(state);
      _state = state;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    public abstract void PerformSetTunerState(TunerState state);

    #endregion

    #region scan/tune

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public abstract bool CanTune(IChannel channel);

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the sub-channel associated with the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the sub-channel associated with the tuned channel</returns>
    public virtual ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      this.LogDebug("tuner base: tune channel, {0}", channel);
      ITvSubChannel subChannel = null;
      try
      {
        // The tuner must be loaded before a channel can be tuned.
        if (_state == TunerState.NotLoaded)
        {
          Load();
          ThrowExceptionIfTuneCancelled();
        }
        // Some tuners (for example: CableCARD tuners) are only able to
        // deliver one service... full stop.
        else if (!_supportsSubChannels && _mapSubChannels.Count > 0)
        {
          if (_mapSubChannels.TryGetValue(subChannelId, out subChannel))
          {
            // Existing sub-channel.
            if (_mapSubChannels.Count != 1)
            {
              // If this is not the only sub-channel then by definition this
              // must be an attempt to tune a new service. Not allowed.
              throw new TvException("Tuner is not able to receive more than one service.");
            }
          }
          else
          {
            // New sub-channel.
            Dictionary<int, ITvSubChannel>.ValueCollection.Enumerator en = _mapSubChannels.Values.GetEnumerator();
            en.MoveNext();
            if (en.Current.CurrentChannel != channel)
            {
              // The tuner is currently streaming a different service.
              throw new TvException("Tuner is not able to receive more than one service.");
            }
          }
        }

        // Get a sub-channel for the service.
        string description;
        if (subChannel == null && !_mapSubChannels.TryGetValue(subChannelId, out subChannel))
        {
          description = "creating new sub-channel";
          subChannelId = _nextSubChannelId++;
          subChannel = CreateNewSubChannel(subChannelId);
          _mapSubChannels[subChannelId] = subChannel;
          FireNewSubChannelEvent(subChannelId);
        }
        else
        {
          description = "using existing sub-channel";
          // If reusing a sub-channel and our multi-channel decrypt mode is
          // "changes", tell the extension to stop decrypting the previous
          // service before we lose access to the PMT and CAT.
          if (_multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
          {
            UpdateDecryptList(subChannelId, CaPmtListManagementAction.Last);
          }
        }
        this.LogInfo("tuner base: {0}, ID = {1}, count = {2}", description, subChannelId, _mapSubChannels.Count);
        subChannel.CurrentChannel = channel;

        // Sub-channel OnBeforeTune().
        subChannel.OnBeforeTune();

        // Do we need to tune?
        bool tuned = false;
        if (_currentTuningDetail == null || _currentTuningDetail.IsDifferentTransponder(channel))
        {
          tuned = true;
          // Stop the EPG grabber. We're going to move to a different channel. Any EPG data that
          // has been grabbed but not stored is thrown away.
          if (InternalEpgGrabberInterface != null && InternalEpgGrabberInterface.IsEpgGrabbing)
          {
            InternalEpgGrabberInterface.AbortGrabbing();
          }

          // When we call ICustomDevice.OnBeforeTune(), the ICustomDevice may modify the tuning parameters.
          // However, the original channel object *must not* be modified otherwise IsDifferentTransponder()
          // will sometimes returns true when it shouldn't. See mantis 0002979.
          IChannel tuneChannel = channel.GetTuningChannel();

          // Extension OnBeforeTune().
          TunerAction action = TunerAction.Default;
          foreach (ICustomDevice extension in _extensions)
          {
            TunerAction extensionAction;
            extension.OnBeforeTune(this, _currentTuningDetail, ref tuneChannel, out extensionAction);
            if (extensionAction != TunerAction.Unload && extensionAction != TunerAction.Default)
            {
              // Valid action requested...
              if (extensionAction > action)
              {
                this.LogDebug("tuner base: extension \"{0}\" overrides action {1} with {2}", extension.Name, action, extensionAction);
                action = extensionAction;
              }
              else if (extensionAction != action)
              {
                this.LogWarn("tuner base: extension \"{0}\" wants to perform action {1}, overriden", extension.Name, extensionAction);
              }
            }

            // Turn on power. This usually needs to happen before tuning.
            IPowerDevice powerDevice = extension as IPowerDevice;
            if (powerDevice != null)
            {
              powerDevice.SetPowerState(PowerState.On);
            }
          }
          ThrowExceptionIfTuneCancelled();
          if (action != TunerAction.Default)
          {
            PerformTunerAction(action);
          }

          // Send DiSEqC commands (if necessary) before actually tuning in case the driver applies the commands
          // during the tuning process.
          if (_diseqcController != null)
          {
            _diseqcController.SwitchToChannel(channel as DVBSChannel);
          }

          // Apply tuning parameters.
          ThrowExceptionIfTuneCancelled();
          _isSignalLocked = false;
          if (_useCustomTuning)
          {
            foreach (ICustomDevice extension in _extensions)
            {
              ICustomTuner customTuner = extension as ICustomTuner;
              if (customTuner != null && customTuner.CanTuneChannel(channel))
              {
                this.LogDebug("tuner base: using custom tuning");
                if (!customTuner.Tune(tuneChannel))
                {
                  ThrowExceptionIfTuneCancelled();
                  this.LogWarn("tuner base: custom tuning failed, falling back to default tuning");
                  PerformTuning(tuneChannel);
                }
                break;
              }
            }
          }
          else
          {
            PerformTuning(tuneChannel);
          }
          ResetSignalUpdate();
          ThrowExceptionIfTuneCancelled();

          // Extension OnAfterTune().
          foreach (ICustomDevice extension in _extensions)
          {
            extension.OnAfterTune(this, channel);
          }
        }

        // Sub-channel OnAfterTune().
        subChannel.OnAfterTune();

        _currentTuningDetail = channel;

        // Start the tuner.
        ThrowExceptionIfTuneCancelled();
        PerformTunerAction(TunerAction.Start);
        ThrowExceptionIfTuneCancelled();

        // Extension OnStarted().
        foreach (ICustomDevice extension in _extensions)
        {
          extension.OnStarted(this, channel);
        }

        LockInOnSignal();

        subChannel.AfterTuneEvent -= FireAfterTuneEvent;
        subChannel.AfterTuneEvent += FireAfterTuneEvent;

        // Ensure that data/streams which are required to detect the service will pass through the
        // tuner's PID filter.
        ConfigurePidFilter(tuned);
        ThrowExceptionIfTuneCancelled();

        subChannel.OnGraphRunning();

        // At this point we should know which data/streams form the service(s) that are being
        // accessed. We need to ensure those streams will pass through the tuner's PID filter.
        ThrowExceptionIfTuneCancelled();
        ConfigurePidFilter();
        ThrowExceptionIfTuneCancelled();

        // If the service is encrypted, start decrypting it.
        UpdateDecryptList(subChannelId, CaPmtListManagementAction.Add);
      }
      catch (Exception ex)
      {
        if (!(ex is TvException))
        {
          this.LogError(ex);
        }

        // One potential reason for getting here is that signal could not be locked, and the reason for
        // that may be that tuning failed. We always want to force a retune on the next tune request in
        // this situation.
        _currentTuningDetail = null;
        if (subChannel != null)
        {
          FreeSubChannel(subChannelId);
        }
        throw;
      }
      finally
      {
        _cancelTune = false;
      }

      return subChannel;
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    /// <param name="subChannelId">The ID of the sub-channel associated with the channel that is being cancelled.</param>
    public void CancelTune(int subChannelId)
    {
      this.LogDebug("tuner base: sub-channel {0} cancel tune", subChannelId);
      _cancelTune = true;
      ITvSubChannel subChannel;
      if (_mapSubChannels.TryGetValue(subChannelId, out subChannel))
      {
        subChannel.CancelTune();
      }
    }

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw an exception if it has.
    /// </summary>
    private void ThrowExceptionIfTuneCancelled()
    {
      if (_cancelTune)
      {
        throw new TvExceptionTuneCancelled();
      }
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public abstract void PerformTuning(IChannel channel);

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public abstract IList<ICustomDevice> PerformLoading();

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    public abstract void PerformUnloading();

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <remarks>
    /// The <paramref name="onlyGetLock"/> parameter exists as a speed
    /// optimisation. Getting strength and quality readings can be slow.
    /// </remarks>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    public abstract void GetSignalStatus(bool onlyGetLock, out bool isLocked, out bool isPresent, out int strength, out int quality);

    #endregion

    #region quality control

    /// <summary>
    /// Get the tuner's quality control interface.
    /// </summary>
    public virtual IQuality Quality
    {
      get
      {
        return _encoderController;
      }
    }

    #endregion

    #region channel linkages

    /// <summary>
    /// Starts scanning for linkages.
    /// </summary>
    /// <param name="callBack">The delegate to call when scanning is complete or canceled.</param>
    public virtual void StartLinkageScanner(BaseChannelLinkageScanner callBack)
    {
    }

    /// <summary>
    /// Stop/reset the linkage scanner.
    /// </summary>
    public virtual void ResetLinkageScanner()
    {
    }

    /// <summary>
    /// Get the portal channels found by the linkage scanner.
    /// </summary>
    public virtual List<PortalChannel> ChannelLinkages
    {
      get
      {
        return null;
      }
    }

    #endregion

    #region sub-channel management

    /// <summary>
    /// Allocate a new sub-channel instance.
    /// </summary>
    /// <param name="id">The identifier for the sub-channel.</param>
    /// <returns>the new sub-channel instance</returns>
    public abstract ITvSubChannel CreateNewSubChannel(int id);

    /// <summary>
    /// Free a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel identifier.</param>
    public void FreeSubChannel(int id)
    {
      this.LogDebug("tuner base: free sub-channel, ID = {0}, count = {1}", id, _mapSubChannels.Count);
      ITvSubChannel subChannel;
      if (_mapSubChannels.TryGetValue(id, out subChannel))
      {
        if (subChannel.IsTimeShifting)
        {
          this.LogError("tuner base: asked to free sub-channel that is still timeshifting!");
          return;
        }
        if (subChannel.IsRecording)
        {
          this.LogError("tuner base: asked to free sub-channel that is still recording!");
          return;
        }

        try
        {
          UpdateDecryptList(id, CaPmtListManagementAction.Last);
          subChannel.Decompose();
        }
        finally
        {
          _mapSubChannels.Remove(id);
        }
        // PID filters are configured according to the PIDs that need to be passed, so reconfigure the PID
        // filter *after* we removed the sub-channel (otherwise PIDs for the sub-channel that we're freeing
        // won't be removed).
        ConfigurePidFilter();
      }
      else
      {
        this.LogWarn("tuner base: sub-channel not found!");
      }
      if (_mapSubChannels.Count == 0)
      {
        this.LogDebug("tuner base: no sub-channels present, stopping tuner");
        _nextSubChannelId = 0;
        Stop();
      }
      else
      {
        this.LogDebug("tuner base: sub-channels still present, leave tuner running");
      }
    }

    /// <summary>
    /// Free all sub-channels.
    /// </summary>
    private void FreeAllSubChannels()
    {
      this.LogInfo("tuner base: free all sub-channels, count = {0}", _mapSubChannels.Count);
      Dictionary<int, ITvSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
      while (en.MoveNext())
      {
        en.Current.Value.Decompose();
      }
      _mapSubChannels.Clear();
      _nextSubChannelId = 0;
    }

    /// <summary>
    /// Get a specific sub-channel.
    /// </summary>
    /// <param name="id">The ID of the sub-channel.</param>
    /// <returns></returns>
    public ITvSubChannel GetSubChannel(int id)
    {
      ITvSubChannel subChannel = null;
      if (_mapSubChannels != null)
      {
        _mapSubChannels.TryGetValue(id, out subChannel);
      }
      return subChannel;
    }

    /// <summary>
    /// Get the tuner's sub-channels.
    /// </summary>
    /// <value>An array containing the sub-channels.</value>
    public ITvSubChannel[] SubChannels
    {
      get
      {
        int count = 0;
        ITvSubChannel[] channels = new ITvSubChannel[_mapSubChannels.Count];
        Dictionary<int, ITvSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          channels[count++] = en.Current.Value;
        }
        return channels;
      }
    }

    #endregion

    #region move me to sub-channel manager

    /// <summary>
    /// The mode to use for controlling tuner PID filter(s).
    /// </summary>
    /// <remarks>
    /// This setting can be used to enable or disable the tuner's PID filter even when the tuning context
    /// (for example, DVB-S vs. DVB-S2) would usually result in different behaviour. Note that it is usually
    /// not ideal to have to manually enable or disable a PID filter as it can affect tuning reliability.
    /// </remarks>
    private PidFilterMode _pidFilterMode = PidFilterMode.Auto;
    private HashSet<ushort> _previousPids = new HashSet<ushort>();

    /// <summary>
    /// Configure the tuner's PID filter(s) to enable receiving the PIDs for each of the current sub-channels.
    /// </summary>
    private void ConfigurePidFilter(bool isTune = false)
    {
      this.LogDebug("Mpeg2TunerController: configure PID filter, mode = {0}", _pidFilterMode);

      if (_mapSubChannels == null || _mapSubChannels.Count == 0)
      {
        this.LogDebug("Mpeg2TunerController: no sub-channels");
        return;
      }

      HashSet<ushort> pidSet = null;
      HashSet<ushort> pidsOverflow = new HashSet<ushort>();
      HashSet<ushort> pidsToAdd = null;
      HashSet<ushort> pidsToRemove = null;
      foreach (ICustomDevice e in _extensions)
      {
        IMpeg2PidFilter filter = e as IMpeg2PidFilter;
        if (filter == null)
        {
          continue;
        }

        this.LogDebug("Mpeg2TunerController: found PID filter controller interface \"{0}\"", filter.Name);

        if (_pidFilterMode == PidFilterMode.Disabled || (_pidFilterMode == PidFilterMode.Auto && !filter.ShouldEnableFilter(_currentTuningDetail)))
        {
          filter.DisableFilter();
          _previousPids.Clear();
          return;
        }

        if (isTune)
        {
          _previousPids.Clear();
        }
        this.LogDebug("Mpeg2TunerController: current, count = {0}, PIDs = {1}", _previousPids.Count, string.Join(", ", _previousPids));
        pidSet = new HashSet<ushort>();
        foreach (ITvSubChannel subChannel in _mapSubChannels.Values)
        {
          SubChannelMpeg2Ts dvbChannel = subChannel as SubChannelMpeg2Ts;
          if (dvbChannel != null && dvbChannel.Pids != null)
          {
            // Build a distinct super-set of PIDs used by the sub-channels.
            pidSet.UnionWith(dvbChannel.Pids);
          }
        }
        this.LogDebug("Mpeg2TunerController: required, count = {0}, PIDs = {1}", pidSet.Count, string.Join(", ", pidSet));

        bool tooManyPids = (filter.MaximumPidCount > 0 && pidSet.Count > filter.MaximumPidCount);
        if (tooManyPids && _pidFilterMode == PidFilterMode.Auto)
        {
          this.LogDebug("Mpeg2TunerController: PID count exceeds filter limit {0}, disabling filter", filter.MaximumPidCount);
          filter.DisableFilter();
          _previousPids.Clear();
          return;
        }

        pidsToAdd = new HashSet<ushort>(pidSet);
        pidsToAdd.ExceptWith(_previousPids);
        pidsToRemove = new HashSet<ushort>(_previousPids);
        pidsToRemove.ExceptWith(pidSet);
        if (pidsToAdd.Count == 0 && pidsToRemove.Count == 0)
        {
          this.LogDebug("Mpeg2TunerController: nothing to do");
          return;
        }

        if (pidsToRemove.Count > 0)
        {
          this.LogDebug("Mpeg2TunerController: remove, count = {0}, PIDs = {1}", pidsToRemove.Count, string.Join(", ", pidsToRemove));
          filter.AllowStreams(pidsToRemove);
          _previousPids.ExceptWith(pidsToRemove);
          if (pidsToAdd.Count == 0)
          {
            filter.ApplyFilter();
            return;
          }
        }

        if (tooManyPids)
        {
          HashSet<ushort> pidsAdded = new HashSet<ushort>();
          foreach (ushort pid in pidsToAdd)
          {
            if (_previousPids.Count >= filter.MaximumPidCount)
            {
              break;
            }
            pidsAdded.Add(pid);
          }
          this.LogDebug("Mpeg2TunerController: add, count = {0}, PIDs = {1}", pidsAdded.Count, string.Join(", ", pidsAdded));
          filter.AllowStreams(pidsAdded);
          _previousPids.UnionWith(pidsAdded);
          pidsToAdd.ExceptWith(pidsAdded);
          this.LogDebug("Mpeg2TunerController: overflow, count = {0}, PIDs = {1}", pidsToAdd.Count, string.Join(", ", pidsToAdd));
        }
        else
        {
          this.LogDebug("Mpeg2TunerController: add, count = {0}, PIDs = {1}", pidsToAdd.Count, string.Join(", ", pidsToAdd));
          filter.AllowStreams(pidsToAdd);
          _previousPids.UnionWith(pidsToAdd);
        }
        filter.ApplyFilter();
        return;
      }
    }

    /// <summary>
    /// Update the list of services being decrypted by the device's conditional access interfaces(s).
    /// </summary>
    /// <remarks>
    /// The strategy here is usually to only send commands to the CAM when we need an *additional* service
    /// to be decrypted. The *only* exception is when we have to stop decrypting services in "changes" mode.
    /// We don't send "not selected" commands for "list" or "only" mode because this can disrupt the other
    /// services that still need to be decrypted. We also don't send "keep decrypting" commands (alternative
    /// to "not selected") because that will almost certainly cause glitches in streams.
    /// </remarks>
    /// <param name="subChannelId">The ID of the sub-channel causing this update.</param>
    /// <param name="updateAction"><c>Add</c> if the sub-channel is being tuned, <c>update</c> if the PMT for the
    ///   sub-channel has changed, or <c>last</c> if the sub-channel is being disposed.</param>
    private void UpdateDecryptList(int subChannelId, CaPmtListManagementAction updateAction)
    {
      if (!_useConditionalAccessInterface)
      {
        this.LogWarn("Mpeg2TunerController: CA disabled");
        return;
      }
      if (_caProviders.Count == 0)
      {
        this.LogWarn("Mpeg2TunerController: no CA providers identified");
        return;
      }
      this.LogDebug("Mpeg2TunerController: sub-channel {0} update decrypt list, mode = {1}, update action = {2}", subChannelId, _multiChannelDecryptMode, updateAction);

      if (_mapSubChannels == null || _mapSubChannels.Count == 0 || !_mapSubChannels.ContainsKey(subChannelId))
      {
        this.LogDebug("Mpeg2TunerController: sub-channel not found");
        return;
      }
      if (_mapSubChannels[subChannelId].CurrentChannel.FreeToAir)
      {
        this.LogDebug("Mpeg2TunerController: service is not encrypted");
        return;
      }
      if (updateAction == CaPmtListManagementAction.Last && _multiChannelDecryptMode != MultiChannelDecryptMode.Changes)
      {
        this.LogDebug("Mpeg2TunerController: \"not selected\" command acknowledged, no action required");
        return;
      }

      // First build a distinct list of the services that we need to handle.
      this.LogDebug("Mpeg2TunerController: assembling service list");
      List<ITvSubChannel> distinctServices = new List<ITvSubChannel>();
      Dictionary<int, ITvSubChannel>.ValueCollection.Enumerator en = _mapSubChannels.Values.GetEnumerator();
      DVBBaseChannel updatedDigitalService = _mapSubChannels[subChannelId].CurrentChannel as DVBBaseChannel;
      AnalogChannel updatedAnalogService = _mapSubChannels[subChannelId].CurrentChannel as AnalogChannel;
      while (en.MoveNext())
      {
        IChannel service = en.Current.CurrentChannel;
        // We don't care about FTA services here.
        if (service.FreeToAir)
        {
          continue;
        }

        // Keep an eye out - if there is another sub-channel accessing the same service as the sub-channel that
        // is being updated then we always do *nothing* unless this is specifically an update request. In any other
        // situation, if we were to stop decrypting the service it would be wrong; if we were to start decrypting
        // the service it would be unnecessary and possibly cause stream interruptions.
        if (en.Current.SubChannelId != subChannelId && updateAction != CaPmtListManagementAction.Update)
        {
          if (updatedDigitalService != null)
          {
            DVBBaseChannel digitalService = service as DVBBaseChannel;
            if (digitalService != null && digitalService.ServiceId == updatedDigitalService.ServiceId)
            {
              this.LogDebug("Mpeg2TunerController: the service for this sub-channel is a duplicate, no action required");
              return;
            }
          }
          else if (updatedAnalogService != null)
          {
            AnalogChannel analogService = service as AnalogChannel;
            if (analogService != null && analogService.Frequency == updatedAnalogService.Frequency && analogService.ChannelNumber == updatedAnalogService.ChannelNumber)
            {
              this.LogDebug("Mpeg2TunerController: the service for this sub-channel is a duplicate, no action required");
              return;
            }
          }
          else
          {
            throw new TvException("Mpeg2TunerController: service type not recognised, unable to assemble decrypt service list\r\n" + service.ToString());
          }
        }

        if (_multiChannelDecryptMode == MultiChannelDecryptMode.List)
        {
          // Check for "list" mode: have we already go this service in our distinct list? If so, don't add it
          // again...
          bool exists = false;
          foreach (ITvSubChannel serviceToDecrypt in distinctServices)
          {
            DVBBaseChannel digitalService = service as DVBBaseChannel;
            if (digitalService != null)
            {
              if (digitalService.ServiceId == ((DVBBaseChannel)serviceToDecrypt.CurrentChannel).ServiceId)
              {
                exists = true;
                break;
              }
            }
            else
            {
              AnalogChannel analogService = service as AnalogChannel;
              if (analogService != null)
              {
                if (analogService.Frequency == ((AnalogChannel)serviceToDecrypt.CurrentChannel).Frequency &&
                  analogService.ChannelNumber == ((AnalogChannel)serviceToDecrypt.CurrentChannel).ChannelNumber)
                {
                  exists = true;
                  break;
                }
              }
              else
              {
                throw new TvException("Mpeg2TunerController: service type not recognised, unable to assemble decrypt service list\r\n" + service.ToString());
              }
            }
          }
          if (!exists)
          {
            distinctServices.Add(en.Current);
          }
        }
        else if (en.Current.SubChannelId == subChannelId)
        {
          // For "changes" and "only" modes: we only send one command and that relates to the service being updated.
          distinctServices.Add(en.Current);
        }
      }

      // This should never happen, regardless of the action that is being performed. Note that this is just a
      // sanity check. It is expected that the service will manage decrypt limit logic. This check does not work
      // for "changes" mode.
      if (_decryptLimit > 0 && distinctServices.Count > _decryptLimit)
      {
        this.LogError("Mpeg2TunerController: decrypt limit exceeded");
        return;
      }
      if (distinctServices.Count == 0)
      {
        this.LogDebug("Mpeg2TunerController: no services to update");
        return;
      }

      // Send the service list or changes to the CA providers.
      for (int attempt = 1; attempt <= _decryptFailureRetryCount + 1; attempt++)
      {
        ThrowExceptionIfTuneCancelled();
        if (attempt > 1)
        {
          this.LogDebug("Mpeg2TunerController: attempt {0}...", attempt);
        }

        foreach (IConditionalAccessProvider caProvider in _caProviders)
        {
          this.LogDebug("Mpeg2TunerController: CA provider {0}...", caProvider.Name);

          if (_waitUntilCaInterfaceReady && !caProvider.IsConditionalAccessInterfaceReady())
          {
            this.LogDebug("Mpeg2TunerController: provider is not ready, waiting for up to 15 seconds", caProvider.Name);
            DateTime startWait = DateTime.Now;
            TimeSpan waitTime = new TimeSpan(0);
            while (waitTime.TotalMilliseconds < 15000)
            {
              ThrowExceptionIfTuneCancelled();
              System.Threading.Thread.Sleep(200);
              waitTime = DateTime.Now - startWait;
              if (caProvider.IsConditionalAccessInterfaceReady())
              {
                this.LogDebug("Mpeg2TunerController: provider ready after {0} ms", waitTime.TotalMilliseconds);
                break;
              }
            }
          }

          // Ready or not, we send commands now.
          this.LogDebug("Mpeg2TunerController: sending command(s)");
          bool success = true;
          SubChannelMpeg2Ts digitalService;
          // The default action is "more" - this will be changed below if necessary.
          CaPmtListManagementAction action = CaPmtListManagementAction.More;

          // The command is "start/continue descrambling" unless we're removing services.
          CaPmtCommand command = CaPmtCommand.OkDescrambling;
          if (updateAction == CaPmtListManagementAction.Last)
          {
            command = CaPmtCommand.NotSelected;
          }
          for (int i = 0; i < distinctServices.Count; i++)
          {
            ThrowExceptionIfTuneCancelled();
            if (i == 0)
            {
              if (distinctServices.Count == 1)
              {
                if (_multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
                {
                  // Remove a service...
                  if (updateAction == CaPmtListManagementAction.Last)
                  {
                    action = CaPmtListManagementAction.Only;
                  }
                  // Add or update a service...
                  else
                  {
                    action = updateAction;
                  }
                }
                else
                {
                  action = CaPmtListManagementAction.Only;
                }
              }
              else
              {
                action = CaPmtListManagementAction.First;
              }
            }
            else if (i == distinctServices.Count - 1)
            {
              action = CaPmtListManagementAction.Last;
            }
            else
            {
              action = CaPmtListManagementAction.More;
            }

            this.LogDebug("  command = {0}, action = {1}, service = {2}", command, action, distinctServices[i].CurrentChannel.Name);
            digitalService = distinctServices[i] as SubChannelMpeg2Ts;
            if (digitalService == null)
            {
              success &= caProvider.SendConditionalAccessCommand(distinctServices[i].CurrentChannel, action, command, null, null);
            }
            else
            {
              // TODO need to PatchPmtForCam() sometime before now, and in such a way that the patched PMT is not propagated to TsWriter etc.
              success &= caProvider.SendConditionalAccessCommand(distinctServices[i].CurrentChannel, action, command, digitalService.Pmt, digitalService.Cat);
            }
          }

          // Are we done?
          if (success)
          {
            return;
          }
        }
      }
    }

    #endregion
  }
}