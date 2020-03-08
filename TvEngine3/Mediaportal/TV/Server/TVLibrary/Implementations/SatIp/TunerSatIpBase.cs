﻿#region Copyright (C) 2005-2011 Team MediaPortal

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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Implementations.Rtcp;
using Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.CP.Description;
using RtspClient = Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp.RtspClient;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for SAT>IP tuners.
  /// </summary>
  internal abstract class TunerSatIpBase : TunerBase, IMpeg2PidFilter
  {
    #region constants

    private static readonly Regex REGEX_DESCRIBE_RESPONSE_SIGNAL_INFO = new Regex(@";tuner=\d+,(\d+),(\d+),(\d+),", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex REGEX_RTSP_SESSION_HEADER = new Regex(@"\s*([^\s;]+)(;timeout=(\d+))?");
    private const int DEFAULT_RTSP_SESSION_TIMEOUT = 60;    // unit = s
    private const int RTCP_REPORT_WAIT_TIMEOUT = 400;       // unit = ms; specification says the server should deliver 5 reports per second

    #endregion

    #region variables

    /// <summary>
    /// The SAT>IP server's UPnP device descriptor.
    /// </summary>
    protected DeviceDescriptor _serverDescriptor = null;

    /// <summary>
    /// The IP address of the local NIC which is connected to the SAT>IP server.
    /// </summary>
    protected IPAddress _localIpAddress = null;

    /// <summary>
    /// The SAT>IP server's IP address.
    /// </summary>
    protected string _serverIpAddress = string.Empty;

    /// <summary>
    /// An RTSP client, used to communicate with the SAT>IP server.
    /// </summary>
    private RtspClient _rtspClient = null;

    /// <summary>
    /// The current RTSP session ID. Used in the header of all RTSP messages
    /// sent to the server.
    /// </summary>
    private string _rtspSessionId = string.Empty;

    /// <summary>
    /// The time (in seconds) after which the SAT>IP server will stop streaming
    /// if it does not receive some kind of interaction.
    /// </summary>
    private int _rtspSessionTimeout = -1;

    /// <summary>
    /// The current SAT>IP stream ID. Used as part of all URIs sent to the
    /// SAT>IP server.
    /// </summary>
    private string _satIpStreamId = string.Empty;

    /// <summary>
    /// A thread, used to periodically send RTSP OPTIONS to tell the SAT>IP
    /// server not to stop streaming.
    /// </summary>
    private Thread _streamingKeepAliveThread = null;

    /// <summary>
    /// An event, used to stop the streaming keep-alive thread.
    /// </summary>
    private AutoResetEvent _streamingKeepAliveThreadStopEvent = null;

    /// <summary>
    /// A thread, used to listen for RTCP reports containing signal status
    /// updates.
    /// </summary>
    private Thread _rtcpListenerThread = null;

    /// <summary>
    /// An event, used to stop the RTCP listener thread.
    /// </summary>
    private AutoResetEvent _rtcpListenerThreadStopEvent = null;

    /// <summary>
    /// The port on which the RTCP listener thread listens.
    /// </summary>
    private int _rtcpClientPort = -1;

    /// <summary>
    /// The port that the RTCP listener thread listens to.
    /// </summary>
    private int _rtcpServerPort = -1;

    // PID filter control variables
    private bool _isPidFilterDisabled = false;
    private HashSet<ushort> _pidFilterPidsToRemove = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToAdd = new HashSet<ushort>();

    // signal status
    private volatile bool _isSignalLocked = false;
    private int _signalStrength = 0;
    private int _signalQuality = 0;

    /// <summary>
    /// Internal stream tuner, used to receive the RTP stream. This allows us
    /// to decouple the stream tuning implementation (eg. DirectShow) from the
    /// SAT>IP implementation.
    /// </summary>
    private ITunerInternal _streamTuner = null;

    /// <summary>
    /// The tuner's channel scanning interface.
    /// </summary>
    private IChannelScannerInternal _channelScanner = null;

    private int _rtpClientPort;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpBase"/> class.
    /// </summary>
    /// <param name="serverDescriptor">The server's UPnP device description.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    /// <param name="type">The tuner type.</param>
    public TunerSatIpBase(DeviceDescriptor serverDescriptor, int sequenceNumber, ITunerInternal streamTuner, CardType type)
      : base(serverDescriptor.FriendlyName + " Tuner " + sequenceNumber, serverDescriptor.DeviceUUID + sequenceNumber + type.ToString()[type.ToString().Length - 1], type)
    {
      DVBIPChannel streamChannel = new DVBIPChannel();
      streamChannel.Url = "rtp://127.0.0.1";
      if (streamTuner == null || !streamTuner.CanTune(streamChannel))
      {
        throw new TvException("Internal tuner implementation is not usable.");
      }

      _serverDescriptor = serverDescriptor;
      _streamTuner = new TunerInternalWrapper(streamTuner);
      _localIpAddress = serverDescriptor.RootDescriptor.SSDPRootEntry.PreferredLink.Endpoint.EndPointIPAddress;
      _serverIpAddress = new Uri(serverDescriptor.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation).Host;
      _productInstanceId = serverDescriptor.DeviceUUID;
      _tunerInstanceId = sequenceNumber.ToString();
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="parameters">A URI section specifying the tuning parameters.</param>
    protected void PerformTuning(DVBBaseChannel channel, string parameters)
    {
      this.LogDebug("SAT>IP base: perform tuning");

      RtspRequest request;
      RtspResponse response = null;
      string rtpUrl = null;

      bool continueTuning = false;
      if (!string.IsNullOrEmpty(_satIpStreamId))
      {
        // Change channel = RTSP PLAY.
        this.LogDebug("SAT>IP base: send RTSP PLAY");
        string uri = string.Format("rtsp://{0}/stream={1}?{2}", _serverIpAddress, _satIpStreamId, parameters);
        request = new RtspRequest(RtspMethod.Play, uri);
        request.Headers.Add("Session", _rtspSessionId);
        var rtspStatusCode = _rtspClient.SendRequest(request, out response);
        if (rtspStatusCode != RtspStatusCode.Ok)
        {
          if (rtspStatusCode == RtspStatusCode.SessionNotFound)
          {
            this.LogDebug("SAT>IP base: RTSP PLAY unknwon session {0}, start new session", _rtspSessionId);
            EndSession();
            continueTuning = true;
          }
          else
           throw new TvException("Failed to tune, non-OK RTSP PLAY status code {0} {1}", response.StatusCode, response.ReasonPhrase);
        }
        else
          this.LogDebug("SAT>IP base: RTSP PLAY response okay");

        if (!continueTuning)
          return;
      }

      // First tune = RTSP SETUP.
      // Find free ports for receiving the RTP and RTCP streams.
      // We need two adjacent UDP ports. One for RTP; one for RTCP. By convention, the RTP port is even.
      int[] freePorts;
      if (!PortReservation.GetFreePorts(2, out freePorts))
        throw new TvException("SAT>IP base: Failed to get free ports for RTP/RTCP");

      _rtpClientPort = freePorts[0];
      _rtcpClientPort = freePorts[1];

      this.LogDebug("SAT>IP base: send RTSP SETUP, RTP client port = {0}", _rtpClientPort);

      // SETUP a session.
      _rtspClient = new RtspClient(_serverIpAddress);
      request = new RtspRequest(RtspMethod.Setup, string.Format("rtsp://{0}/?{1}&pids=0", _serverIpAddress, parameters));
      request.Headers.Add("Transport", string.Format("RTP/AVP;unicast;client_port={0}-{1}", _rtpClientPort, _rtcpClientPort));
      if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
      {
        throw new TvException("Failed to tune, non-OK RTSP SETUP status code {0} {1}", response.StatusCode, response.ReasonPhrase);
      }

      // Handle the SETUP response.
      // Find the SAT>IP stream ID.
      if (!response.Headers.TryGetValue("com.ses.streamID", out _satIpStreamId))
      {
        throw new TvException("Failed to tune, not able to find stream ID header in RTSP SETUP response");
      }

      // Find the RTSP session ID and timeout.
      string sessionHeader;
      if (!response.Headers.TryGetValue("Session", out sessionHeader))
      {
        throw new TvException("Failed to tune, not able to find session header in RTSP SETUP response");
      }
      Match m = REGEX_RTSP_SESSION_HEADER.Match(sessionHeader);
      if (!m.Success)
      {
        throw new TvException("Failed to tune, RTSP SETUP response session header {0} format not recognised");
      }
      _rtspSessionId = m.Groups[1].Captures[0].Value;
      if (m.Groups[3].Captures.Count == 1)
      {
        _rtspSessionTimeout = int.Parse(m.Groups[3].Captures[0].Value);
      }
      else
      {
        _rtspSessionTimeout = DEFAULT_RTSP_SESSION_TIMEOUT;
      }

      // Find the server's streaming port and check that it registered our
      // preferred local port.
      bool foundRtpTransport = false;
      string rtpServerPort = null;
      string transportHeader;
      if (!response.Headers.TryGetValue("Transport", out transportHeader))
      {
        throw new TvException("Failed to tune, not able to find transport header in RTSP SETUP response");
      }
      string[] transports = transportHeader.Split(',');
      foreach (string transport in transports)
      {
        if (transport.Trim().StartsWith("RTP/AVP"))
        {
          foundRtpTransport = true;
          string[] sections = transport.Split(';');
          foreach (string section in sections)
          {
            string[] parts = section.Split('=');
            if (parts[0].Equals("server_port"))
            {
              string[] ports = parts[1].Split('-');
              rtpServerPort = ports[0];
              _rtcpServerPort = int.Parse(ports[1]);
            }
            else if (parts[0].Equals("client_port"))
            {
              string[] ports = parts[1].Split('-');
              if (!ports[0].Equals(_rtpClientPort.ToString()))
              {
                this.LogWarn("SAT>IP base: server specified RTP client port {0} instead of {1}", ports[0], _rtpClientPort);
              }
              if (!ports[1].Equals(_rtcpClientPort.ToString()))
              {
                this.LogWarn("SAT>IP base: server specified RTCP client port {0} instead of {1}", ports[1], _rtcpClientPort);
              }
              _rtpClientPort = int.Parse(ports[0]);
              _rtcpClientPort = int.Parse(ports[1]);
            }
          }
        }
      }
      if (!foundRtpTransport)
      {
        throw new TvException("Failed to tune, not able to find RTP transport details in RTSP SETUP response transport header");
      }

      // Construct the RTP URL.
      if (string.IsNullOrEmpty(rtpServerPort) || rtpServerPort.Equals("0"))
      {
        rtpUrl = string.Format("rtp://{0}@{1}:{2}", _serverIpAddress, _localIpAddress, _rtpClientPort);
      }
      else
      {
        rtpUrl = string.Format("rtp://{0}:{1}@{2}:{3}", _serverIpAddress, rtpServerPort, _localIpAddress, _rtpClientPort);
      }
      this.LogDebug("SAT>IP base: RTSP SETUP response okay");
      this.LogDebug("  session ID = {0}", _rtspSessionId);
      this.LogDebug("  timeout    = {0}", _rtspSessionTimeout);
      this.LogDebug("  stream ID  = {0}", _satIpStreamId);
      this.LogDebug("  RTP URL    = {0}", rtpUrl);
      this.LogDebug("  RTCP ports = {0}/{1}", _rtcpClientPort, _rtcpServerPort);

      // Configure the stream source filter to receive the RTP stream.
      this.LogDebug("SAT>IP base: configure stream source filter");
      DVBIPChannel streamChannel = new DVBIPChannel();
      streamChannel.Url = rtpUrl;

      // Copy the other channel parameters from the original channel.
      streamChannel.FreeToAir = channel.FreeToAir;
      streamChannel.Frequency = channel.Frequency;
      streamChannel.LogicalChannelNumber = channel.LogicalChannelNumber;
      streamChannel.MediaType = channel.MediaType;
      streamChannel.Name = channel.Name;
      streamChannel.NetworkId = channel.NetworkId;
      streamChannel.PmtPid = channel.PmtPid;
      streamChannel.Provider = channel.Provider;
      streamChannel.ServiceId = channel.ServiceId;
      streamChannel.TransportId = channel.TransportId;

      _streamTuner.PerformTuning(streamChannel);
    }

    #endregion

    #region streaming keep-alive thread

    private void StartStreamingKeepAliveThread()
    {
      // Kill the existing thread if it is in "zombie" state.
      if (_streamingKeepAliveThread != null && !_streamingKeepAliveThread.IsAlive)
      {
        StopStreamingKeepAliveThread();
      }

      if (_streamingKeepAliveThread == null)
      {
        this.LogDebug("SAT>IP base: starting new streaming keep-alive thread");
        _streamingKeepAliveThreadStopEvent = new AutoResetEvent(false);
        _streamingKeepAliveThread = new Thread(new ThreadStart(StreamingKeepAlive));
        _streamingKeepAliveThread.Name = string.Format("SAT>IP tuner {0} streaming keep-alive", TunerId);
        _streamingKeepAliveThread.IsBackground = true;
        _streamingKeepAliveThread.Priority = ThreadPriority.Lowest;
        _streamingKeepAliveThread.Start();
      }
    }

    private void StopStreamingKeepAliveThread()
    {
      if (_streamingKeepAliveThread != null)
      {
        if (!_streamingKeepAliveThread.IsAlive)
        {
          this.LogWarn("SAT>IP base: aborting old streaming keep-alive thread");
          _streamingKeepAliveThread.Abort();
        }
        else
        {
          _streamingKeepAliveThreadStopEvent.Set();
          if (!_streamingKeepAliveThread.Join(_rtspSessionTimeout * 2))
          {
            this.LogWarn("SAT>IP base: failed to join streaming keep-alive thread, aborting thread");
            _streamingKeepAliveThread.Abort();
          }
        }
        _streamingKeepAliveThread = null;
        if (_streamingKeepAliveThreadStopEvent != null)
        {
          _streamingKeepAliveThreadStopEvent.Close();
          _streamingKeepAliveThreadStopEvent = null;
        }
      }
    }

    private void StreamingKeepAlive()
    {
      try
      {
        while (!_streamingKeepAliveThreadStopEvent.WaitOne((_rtspSessionTimeout - 5) * 1000))  // -5 seconds to avoid timeout
        {
          RtspRequest request = new RtspRequest(RtspMethod.Options, string.Format("rtsp://{0}/", _serverIpAddress));
          request.Headers.Add("Session", _rtspSessionId);
          RtspResponse response;
          if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
          {
            this.LogWarn("SAT>IP base: streaming keep-alive request/response failed, non-OK RTSP OPTIONS status code {0} {1}", response.StatusCode, response.ReasonPhrase);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "SAT>IP base: streaming keep-alive thread exception");
        return;
      }
      this.LogDebug("SAT>IP base: streaming keep-alive thread stopping");
    }

    #endregion

    #region RTCP listener thread

    private void StartRtcpListenerThread()
    {
      // Kill the existing thread if it is in "zombie" state.
      if (_rtcpListenerThread != null && !_rtcpListenerThread.IsAlive)
      {
        StopRtcpListenerThread();
      }

      if (_rtcpListenerThread == null)
      {
        this.LogDebug("SAT>IP base: starting new RTCP listener thread");
        _rtcpListenerThreadStopEvent = new AutoResetEvent(false);
        _rtcpListenerThread = new Thread(new ThreadStart(RtcpListener));
        _rtcpListenerThread.Name = string.Format("SAT>IP tuner {0} RTCP listener", TunerId);
        _rtcpListenerThread.IsBackground = true;
        _rtcpListenerThread.Priority = ThreadPriority.Lowest;
        _rtcpListenerThread.Start();
      }
    }

    private void StopRtcpListenerThread()
    {
      if (_rtcpListenerThread != null)
      {
        if (!_rtcpListenerThread.IsAlive)
        {
          this.LogWarn("SAT>IP base: aborting old RTCP listener thread");
          _rtcpListenerThread.Abort();
        }
        else
        {
          _rtcpListenerThreadStopEvent.Set();
          if (!_rtcpListenerThread.Join(RTCP_REPORT_WAIT_TIMEOUT * 2))
          {
            this.LogWarn("SAT>IP base: failed to join RTCP listener thread, aborting thread");
            _rtcpListenerThread.Abort();
          }
        }
        _rtcpListenerThread = null;
        if (_rtcpListenerThreadStopEvent != null)
        {
          _rtcpListenerThreadStopEvent.Close();
          _rtcpListenerThreadStopEvent = null;
        }
      }
    }

    private void RtcpListener()
    {
      try
      {
        bool receivedGoodBye = false;
        UdpClient udpClient = null;
        void InitUdpClient() => udpClient = new UdpClient(new IPEndPoint(_localIpAddress, _rtcpClientPort));
        InitUdpClient();
        try
        {
          udpClient.Client.ReceiveTimeout = RTCP_REPORT_WAIT_TIMEOUT;
          var hasServerPort = _rtcpServerPort > 0;
          IPEndPoint serverEndPoint = hasServerPort
              ? new IPEndPoint(IPAddress.Parse(_serverIpAddress), _rtcpServerPort)
              : new IPEndPoint(IPAddress.Any, 0);

          IPEndPoint receiverEndPoint = new IPEndPoint(IPAddress.Any, 0);
          while (!receivedGoodBye && !_rtcpListenerThreadStopEvent.WaitOne(1))
          {
            byte[] packets = new byte[0];
            try
            {
              packets = udpClient.Receive(ref receiverEndPoint);
            }
            catch (SocketException se)
            {
              udpClient.Close();
              InitUdpClient();
            }

            // Only handle packets from the source we are expecting
            if (hasServerPort && !Equals(receiverEndPoint, serverEndPoint) /* Remote port is known, compare full IPEndPoints */ ||
                !hasServerPort && !Equals(receiverEndPoint.Address.ToString(), _serverIpAddress) /* Remote was not known in advance, only compare IPAddress */)
              continue;

            int offset = 0;
            while (offset < packets.Length)
            {
              // Refer to RFC 3550.
              // https://www.ietf.org/rfc/rfc3550.txt
              switch (packets[offset + 1])
              {
                case 200: //sr
                  var sr = new RtcpSenderReportPacket();
                  sr.Parse(packets, offset);
                  offset += sr.Length;
#if DEBUG_RTCP
                  this.LogDebug(sr.ToString());
#endif
                  break;
                case 201: //rr
                  var rr = new RtcpReceiverReportPacket();
                  rr.Parse(packets, offset);
                  offset += rr.Length;
#if DEBUG_RTCP
                  this.LogDebug(rr.ToString());
#endif
                  break;
                case 202: //sd
                  var sd = new RtcpSourceDescriptionPacket();
                  sd.Parse(packets, offset);
                  offset += sd.Length;
#if DEBUG_RTCP
                  this.LogDebug(sd.ToString());
#endif
                  break;
                case 203: // bye
                  var bye = new RtcpByePacket();
                  bye.Parse(packets, offset);
                  receivedGoodBye = true;
                  offset += bye.Length;
#if DEBUG_RTCP
                  this.LogDebug(bye.ToString());
#endif
                  break;
                case 204: // app
                  var app = new RtcpAppPacket();
                  app.Parse(packets, offset);
                  if (app.Name.Equals("SES1"))
                  {
                    Match m = REGEX_DESCRIBE_RESPONSE_SIGNAL_INFO.Match(app.Data);
                    if (m.Success)
                    {
                      _isSignalLocked = m.Groups[2].Captures[0].Value.Equals("1");
                      _signalStrength = int.Parse(m.Groups[1].Captures[0].Value) * 100 / 255;   // strength: 0..255 => 0..100
                      _signalQuality = int.Parse(m.Groups[3].Captures[0].Value) * 100 / 15;     // quality: 0..15 => 0..100
                    }
                  }
                  offset += app.Length;
#if DEBUG_RTCP
                  this.LogDebug(app.ToString());
#endif
                  break;
              }
            }
          }
        }
        finally
        {
          udpClient.Close();
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "SAT>IP base: RTCP listener thread exception");
        return;
      }
      this.LogDebug("SAT>IP base: RTCP listener thread stopping");
    }

    #endregion

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();
      _streamTuner.ReloadConfiguration();
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ICustomDevice> PerformLoading()
    {
      TunerExtensionLoader loader = new TunerExtensionLoader();
      IList<ICustomDevice> extensions = loader.Load(this, _serverDescriptor);

      // Add the stream tuner extensions to our extensions, but don't re-sort
      // by priority afterwards. This ensures that our extensions are always
      // given first consideration.
      IList<ICustomDevice> streamTunerExtensions = _streamTuner.PerformLoading();
      foreach (ICustomDevice e in streamTunerExtensions)
      {
        extensions.Add(e);
      }

      _channelScanner = _streamTuner.InternalChannelScanningInterface;
      if (_channelScanner != null)
      {
        _channelScanner.Tuner = this;
        _channelScanner.Helper = new ChannelScannerHelperDvb();
      }
      return extensions;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    public override void PerformSetTunerState(TunerState state)
    {
      this.LogDebug("SAT>IP base: perform set tuner state");

      RtspRequest request = null;
      RtspResponse response = null;

      if (_rtspClient != null && !string.IsNullOrEmpty(_satIpStreamId) && !string.IsNullOrEmpty(_rtspSessionId))
      {
        if (state == TunerState.Started)
        {
          // PLAY to start a previously SETUP stream.
          string pidFilterPhrase = "0";   // If you use "none" the source filter will not receive data => fail to start graph.
          if (_isPidFilterDisabled)
          {
            pidFilterPhrase = "all";
          }

          string uri = string.Format("rtsp://{0}:554/stream={1}?pids={2}", _serverIpAddress, _satIpStreamId, pidFilterPhrase);
          request = new RtspRequest(RtspMethod.Play, uri);
          request.Headers.Add("Session", _rtspSessionId);
          if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
          {
            throw new TvException("Failed to start tuner, non-OK RTSP PLAY status code {0} {1}", response.StatusCode, response.ReasonPhrase);
          }

          StartStreamingKeepAliveThread();
          StartRtcpListenerThread();
        }
        else if (state == TunerState.Stopped)
        {
          StopStreamingKeepAliveThread();
          StopRtcpListenerThread();

          request = new RtspRequest(RtspMethod.Teardown, string.Format("rtsp://{0}/stream={1}", _serverIpAddress, _satIpStreamId));
          request.Headers.Add("Session", _rtspSessionId);
          if (_rtspClient.SendRequest(request, out response) != RtspStatusCode.Ok)
          {
            throw new TvException("Failed to stop tuner, non-OK RTSP TEARDOWN status code {0} {1}.", response.StatusCode, response.ReasonPhrase);
          }

          EndSession();
        }
      }

      try
      {
        _streamTuner?.PerformSetTunerState(state);
      }
      catch
      {
        // It isn't possible to start the stream tuner if there is no stream.
        // No signal => no stream.
        if (state != TunerState.Started)
        {
          throw;
        }
      }
    }

    private void EndSession()
    {
      PortReservation.ReleasePort(_rtpClientPort);
      PortReservation.ReleasePort(_rtcpClientPort);

      _rtspClient?.Dispose();
      _rtspClient = null;
      _satIpStreamId = string.Empty;
      _rtspSessionId = string.Empty;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    public override void PerformUnloading()
    {
      _channelScanner = null;
      _streamTuner?.PerformUnloading();
    }

    #endregion

    #region tuning

    /// <summary>
    /// Allocate a new sub-channel instance.
    /// </summary>
    /// <param name="id">The identifier for the sub-channel.</param>
    /// <returns>the new sub-channel instance</returns>
    public override ITvSubChannel CreateNewSubChannel(int id)
    {
      return _streamTuner.CreateNewSubChannel(id);
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    public override void GetSignalStatus(bool onlyGetLock, out bool isLocked, out bool isPresent, out int strength, out int quality)
    {
      isLocked = _isSignalLocked;
      isPresent = _isSignalLocked;
      strength = _signalStrength;
      quality = _signalQuality;
    }

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override IChannelScannerInternal InternalChannelScanningInterface
    {
      get
      {
        return _channelScanner;
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public override IEpgGrabber InternalEpgGrabberInterface
    {
      get
      {
        return _streamTuner?.InternalEpgGrabberInterface;
      }
    }

    #endregion

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public byte Priority
    {
      get
      {
        return 50;
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      // This is a "special" implementation. We do initialisation in other functions.
      return true;
    }

    #region device state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnLoaded(ITVCard tuner, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before the device is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnAfterTune(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the tuner is started but
    /// before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnStarted(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    public virtual void OnStop(ITVCard tuner, ref TunerAction action)
    {
    }

    #endregion

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for the current multiplex.
    /// </summary>
    /// <param name="tuningDetail">The current multiplex/transponder tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    public bool ShouldEnableFilter(IChannel tuningDetail)
    {
      // SAT>IP tuners are networked tuners. It is desirable to enable PID filtering in order to
      // reduce the network bandwidth used.
      return true;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    public bool DisableFilter()
    {
      if (!_isPidFilterDisabled)
      {
        this.LogDebug("SAT>IP base: disable PID filter");
        _isPidFilterDisabled = ConfigurePidFilter("pids=all");
        if (_isPidFilterDisabled)
        {
          _pidFilterPidsToRemove.Clear();
          _pidFilterPidsToAdd.Clear();
        }
      }
      return _isPidFilterDisabled;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    public int MaximumPidCount
    {
      get
      {
        return -1;  // maximum not known
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.UnionWith(pids);
      _pidFilterPidsToRemove.ExceptWith(pids);
      return true;
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.ExceptWith(pids);
      _pidFilterPidsToRemove.UnionWith(pids);
      return true;
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    public bool ApplyFilter()
    {
      string uri = null;
      if (_pidFilterPidsToAdd.Count > 0 && _pidFilterPidsToRemove.Count > 0)
      {
        uri = string.Format("addpids={0}&delpids={1}", string.Join(",", _pidFilterPidsToAdd), string.Join(",", _pidFilterPidsToRemove));
      }
      else if (_pidFilterPidsToAdd.Count > 0)
      {
        uri = "addpids=" + string.Join(",", _pidFilterPidsToAdd);
      }
      else if (_pidFilterPidsToRemove.Count > 0)
      {
        uri = "delpids=" + string.Join(",", _pidFilterPidsToRemove);
      }
      else
      {
        return true;
      }
      this.LogDebug("SAT>IP base: apply PID filter");
      bool result = ConfigurePidFilter(uri);
      if (result)
      {
        _pidFilterPidsToAdd.Clear();
        _pidFilterPidsToRemove.Clear();
        _isPidFilterDisabled = false;
      }
      return result;
    }

    private bool ConfigurePidFilter(string parameters)
    {
      try
      {
        RtspRequest request = new RtspRequest(RtspMethod.Play, string.Format("rtsp://{0}:554/stream={1}?{2}", _serverIpAddress, _satIpStreamId, parameters));
        request.Headers.Add("Accept", "application/sdp");
        request.Headers.Add("Session", _rtspSessionId);
        RtspResponse response = null;
        if (_rtspClient.SendRequest(request, out response) == RtspStatusCode.Ok)
        {
          this.LogDebug("SAT>IP base: result = success");
          return true;
        }

        this.LogError("SAT>IP base: failed to configure PID Filter, non-OK RTSP PLAY status code {0} {1}", response.StatusCode, response.ReasonPhrase);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "SAT>IP base: exception configuring PID filter");
      }
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      _rtspClient?.Dispose();
      _rtspClient = null;
      _streamTuner?.Dispose();
      _streamTuner = null;
    }

    #endregion
  }
}