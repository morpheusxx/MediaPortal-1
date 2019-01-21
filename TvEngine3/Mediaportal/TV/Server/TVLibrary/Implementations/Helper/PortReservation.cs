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
using System.Linq;
using System.Net.NetworkInformation;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Helper
{
  /// <summary>
  /// Helper class to coordinate port allocations.
  /// </summary>
  internal class PortReservation
  {
    private static readonly HashSet<int> _reservedPorts = new HashSet<int>();

    /// <summary>
    /// Returns a specific number of consecutive ports that are available to create new connections.
    /// This method checks both currently used TCP or UDP ports and an own list of reservations.
    /// Use <see cref="ReleasePort"/> to remove formerly reserved port.
    /// </summary>
    /// <param name="numPorts">Number of requested ports</param>
    /// <param name="ports">Returns the free ports</param>
    /// <param name="fromPort">Optional start port to test</param>
    /// <param name="toPort">Optional end port to test</param>
    /// <returns></returns>
    public static bool GetFreePorts(int numPorts, out int[] ports, int fromPort = 40000, int toPort = 65534)
    {
      var activePorts = new HashSet<int>(IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
          .Select(c => c.LocalEndPoint)
          .Union(IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners())
          .Select(ep => ep.Port));

      List<int> freePorts = new List<int>(numPorts);
      lock (_reservedPorts)
      {
        for (var port = fromPort; port <= toPort; port += numPorts)
        {
          bool allFree = true;
          // Check if first and all next ports are free.
          for (int i = 0; i < numPorts; i++)
          {
            var testPort = port + i;
            allFree &= !_reservedPorts.Contains(testPort) && !activePorts.Contains(testPort);
          }

          if (allFree)
          {
            // Remember ports that are not yet bound, but will be used by caller soon.
            for (int i = 0; i < numPorts; i++)
            {
              _reservedPorts.Add(port + i);
              freePorts.Add(port + i);
            }
            ports = freePorts.ToArray();
            return true;
          }
        }
      }

      ports = null;
      return false;
    }

    /// <summary>
    /// Removes a port from the list of reservation, so it can be used later again.
    /// </summary>
    /// <param name="port">Port number</param>
    public static void ReleasePort(int port)
    {
      lock (_reservedPorts)
        _reservedPorts.Remove(port);
    }

    /// <summary>
    /// Removes a list of ports from the list of reservation, so they can be used later again.
    /// </summary>
    /// <param name="ports">Port numbers</param>
    public void ReleasePorts(IEnumerable<int> ports)
    {
      lock (_reservedPorts)
        foreach (int port in ports)
          _reservedPorts.Remove(port);
    }
  }
}