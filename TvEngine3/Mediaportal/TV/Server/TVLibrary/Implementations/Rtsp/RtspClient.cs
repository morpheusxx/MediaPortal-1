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
using System.Net.Sockets;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp
{
  /// <summary>
  /// A simple implementation of an RTSP client.
  /// </summary>
  internal class RtspClient : IDisposable
  {
    #region variables

    private readonly string _serverHost = null;
    private readonly int _serverPort = -1;
    private TcpClient _client = null;
    private int _cseq = 1;
    private readonly object _lockObject = new object();
    private NetworkStream _stream;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="RtspClient"/> class.
    /// </summary>
    /// <param name="serverHost">The RTSP server host name or IP address.</param>
    /// <param name="serverPort">The port on which the RTSP server is listening.</param>
    public RtspClient(string serverHost, int serverPort = 554)
    {
      _serverHost = serverHost;
      _serverPort = serverPort;
    }

    ~RtspClient()
    {
      Dispose();
    }

    public void Dispose()
    {
      lock (_lockObject)
      {
        if (_client != null)
        {
          _client.Close();
          _client = null;
        }
      }
    }

    /// <summary>
    /// Send an RTSP request and retrieve the response.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="response">The response.</param>
    /// <returns>the response status code</returns>
    public RtspStatusCode SendRequest(RtspRequest request, out RtspResponse response)
    {
      response = null;
      lock (_lockObject)
      {
        int retryCount = 0;
        while (true)
        {
          int byteCount = 0;
          byte[] responseBytes = null;
          try
          {
            if (_client == null || _stream == null)
            {
              _client = new TcpClient(_serverHost, _serverPort);
              _stream = _client.GetStream();
            }

            // Send the request and get the response.
            request.Headers["CSeq"] = _cseq.ToString();
            byte[] requestBytes = request.Serialise();
            _stream.Write(requestBytes, 0, requestBytes.Length);
            _cseq++;

            responseBytes = new byte[_client.ReceiveBufferSize];
            byteCount = _stream.Read(responseBytes, 0, responseBytes.Length);
            response = RtspResponse.Deserialise(responseBytes, byteCount);
          }
          catch (Exception ex)
          {
            _stream?.Close();
            _client?.Close();
            _client = null;
            _stream = null;
            if (retryCount == 1)
            {
              this.LogError(ex, "RTSP: failed to open stream to server");
              return RtspStatusCode.RequestTimeOut;
            }

            retryCount++;
            continue;
          }

          // Did we get the whole response?
          string contentLengthString;
          int contentLength = 0;
          if (response.Headers.TryGetValue("Content-Length", out contentLengthString))
          {
            contentLength = int.Parse(contentLengthString);
            if ((string.IsNullOrEmpty(response.Body) && contentLength > 0) || response.Body.Length < contentLength)
            {
              if (response.Body == null)
              {
                response.Body = string.Empty;
              }

              while (byteCount > 0 && response.Body.Length < contentLength)
              {
                byteCount = _stream.Read(responseBytes, 0, responseBytes.Length);
                response.Body += System.Text.Encoding.UTF8.GetString(responseBytes, 0, byteCount);
              }
            }
          }

          return response.StatusCode;
        }
      }
    }
  }
}