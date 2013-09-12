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

using System.Collections.Generic;
using TvLibrary.Channels;
using TvLibrary.Interfaces;


namespace TvLibrary.Implementations.DVB
{
  ///<summary>
  /// A class for holding CA context information.
  ///</summary>
  public class ConditionalAccessContext
  {
    private CamType _camType;
    private DVBBaseChannel _channel;
    private byte[] _pmt;
    private int _pmtLength;
    private int _audioPid;
    private List<ushort> _pids;

    /// <summary>
    /// Gets or sets the type of the CAM.
    /// </summary>
    /// <value>The type of the CAM.</value>
    public CamType CamType
    {
      get { return _camType; }
      set { _camType = value; }
    }

    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    /// <value>The channel.</value>
    public DVBBaseChannel Channel
    {
      get { return _channel; }
      set { _channel = value; }
    }

    /// <summary>
    /// Gets or sets the PMT.
    /// </summary>
    /// <value>The PMT.</value>
    public byte[] Pmt
    {
      get { return _pmt; }
      set { _pmt = value; }
    }

    /// <summary>
    /// Gets or sets the length of the PMT.
    /// </summary>
    /// <value>The length of the PMT.</value>
    public int PmtLength
    {
      get { return _pmtLength; }
      set { _pmtLength = value; }
    }

    /// <summary>
    /// Gets or sets the audio PID.
    /// </summary>
    /// <value>The audio PID.</value>
    public int AudioPid
    {
      get { return _audioPid; }
      set { _audioPid = value; }
    }

    /// <summary>
    /// Gets or sets the PID list.
    /// </summary>
    /// <value>The PID list.</value>
    public List<ushort> Pids
    {
      get { return _pids; }
      set { _pids = value; }
    }
  }
}