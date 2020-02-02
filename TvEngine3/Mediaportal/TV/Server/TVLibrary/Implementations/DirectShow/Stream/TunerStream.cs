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
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which supports the MediaPortal
  /// stream source filter.
  /// </summary>
  internal class TunerStream : TunerDirectShowBase
  {
    /// <summary>
    /// The MediaPortal IPTV/DVB-IP/URL stream source filter class.
    /// </summary>
    [Guid("d3dd4c59-d3a7-4b82-9727-7b9203eb67c0")]
    private class MediaPortalStreamSource
    {
    }

    #region variables

    /// <summary>
    /// The source filter.
    /// </summary>
    private IBaseFilter _filterStreamSource = null;

    /// <summary>
    /// A media type describing the source stream format. 
    /// </summary>
    private AMMediaType _sourceMediaType = null;

    /// <summary>
    /// The source filter's default URL.
    /// </summary>
    protected string _defaultUrl = string.Empty;

    /// <summary>
    /// The class ID (CLSID) for the source filter main class.
    /// </summary>
    protected Guid _sourceFilterClsid = Guid.Empty;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerStream"/> class.
    /// </summary>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    public TunerStream(int sequenceNumber)
      : this("MediaPortal Stream Source", sequenceNumber)
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerStream"/> class.
    /// </summary>
    /// <param name="name">A short name or description for the tuner.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    public TunerStream(string name, int sequenceNumber)
      : this(name + " " + sequenceNumber, name + " " + sequenceNumber)
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerStream"/> class.
    /// </summary>
    /// <param name="name">The tuner's name.</param>
    /// <param name="externalId">The external identifier for the tuner.</param>
    protected TunerStream(string name, string externalId)
      : base(name, externalId, CardType.DvbIP)
    {
      _defaultUrl = "udp://@0.0.0.0:1234";
      _sourceFilterClsid = typeof(MediaPortalStreamSource).GUID;

      _sourceMediaType = new AMMediaType();
      _sourceMediaType.majorType = MediaType.Stream;
      _sourceMediaType.subType = MediaSubType.Mpeg2Transport;
      _sourceMediaType.unkPtr = IntPtr.Zero;
      _sourceMediaType.sampleSize = 0;
      _sourceMediaType.temporalCompression = false;
      _sourceMediaType.fixedSizeSamples = true;
      _sourceMediaType.formatType = FormatType.None;
      _sourceMediaType.formatSize = 0;
      _sourceMediaType.formatPtr = IntPtr.Zero;
    }

    #endregion

    #region ITunerInternal members

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ICustomDevice> PerformLoading()
    {
      this.LogDebug("DirectShow stream: perform loading");
      InitialiseGraph();

      // Start with the source filter.
      if (_sourceFilterClsid == typeof(MediaPortalStreamSource).GUID)
      {
        var filterPath = PathManager.BuildAssemblyRelativePathForArchitecture("MPIPTVSource.ax");
        _filterStreamSource = FilterGraphTools.AddFilterFromFile(_graph, filterPath, _sourceFilterClsid, Name);
      }
      else
      {
        _filterStreamSource = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, _sourceFilterClsid, Name);
      }

      // Check for and load extensions, adding any additional filters to the graph.
      IBaseFilter lastFilter = _filterStreamSource;
      IList<ICustomDevice> extensions = LoadExtensions(_filterStreamSource, ref lastFilter);

      // Complete the graph.
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();
      return extensions;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    public override void PerformUnloading()
    {
      this.LogDebug("DirectShow stream: perform unloading");
      if (_graph != null)
      {
        _graph.RemoveFilter(_filterStreamSource);
      }
      Release.ComObject("DirectShow stream source filter", ref _filterStreamSource);
      Release.AmMediaType(ref _sourceMediaType);

      CleanUpGraph();
    }

    #endregion

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBIPChannel;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      DVBIPChannel streamChannel = channel as DVBIPChannel;
      if (streamChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }
      int hr = (_filterStreamSource as IFileSourceFilter).Load(streamChannel.Url, _sourceMediaType);
      HResult.ThrowException(hr, "Failed to tune channel.");
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
      isLocked = true;
      isPresent = true;
      strength = 100;
      quality = 100;
    }

    #endregion

    #endregion
  }
}