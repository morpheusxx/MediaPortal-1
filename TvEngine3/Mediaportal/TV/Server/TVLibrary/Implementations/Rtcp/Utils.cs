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

/*  
    Copyright (C) <2007-2017>  <Kay Diefenthal>

    SatIp is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    SatIp is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with SatIp.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Rtcp
{
  public class Utils
  {
    public static int ConvertBCDToInt(byte[] byteData, int index, int count)
    {
      int result = 0;
      int shift = 4;

      for (int nibbleIndex = 0; nibbleIndex < count; nibbleIndex++)
      {
        result = (result * 10) + ((byteData[index] >> shift) & 0x0f);

        if (shift == 4)
          shift = 0;
        else
        {
          shift = 4;
          index++;
        }
      }

      return (result);
    }
    public static string ReadString(byte[] data, int offset, int length)
    {
      string encoding = "utf-8"; // Standard latin alphabet
      List<byte> bytes = new List<byte>();
      for (int i = 0; i < length; i++)
      {
        byte character = data[offset + i];
        bool notACharacter = false;
        if (i == 0)
        {
          if (character < 0x20)
          {
            switch (character)
            {
              case 0x00:
                break;
              case 0x01:
                encoding = "iso-8859-5";
                break;
              case 0x02:
                encoding = "iso-8859-6";
                break;
              case 0x03:
                encoding = "iso-8859-7";
                break;
              case 0x04:
                encoding = "iso-8859-8";
                break;
              case 0x05:
                encoding = "iso-8859-9";
                break;
              default:
                break;
            }
            notACharacter = true;
          }
        }
        if (character < 0x20 || (character >= 0x80 && character <= 0x9F))
        {
          notACharacter = true;
        }
        if (!notACharacter)
        {
          bytes.Add(character);
        }
      }
      Encoding enc = Encoding.GetEncoding(encoding);
      ASCIIEncoding destEnc = new ASCIIEncoding();
      byte[] destBytes = Encoding.Convert(enc, destEnc, bytes.ToArray());
      return destEnc.GetString(destBytes);
    }

    public static int Convert2BytesToInt(byte[] buffer, int offset)
    {
      int temp = (int)buffer[offset];
      temp = (temp * 256) + buffer[offset + 1];

      return (temp);
    }
    public static int Convert3BytesToInt(byte[] buffer, int offset)
    {
      int temp = (int)buffer[offset];
      temp = (temp * 256) + buffer[offset + 1];
      temp = (temp * 256) + buffer[offset + 2];

      return (temp);
    }
    public static int Convert4BytesToInt(byte[] buffer, int offset)
    {
      int temp = (int)buffer[offset];
      temp = (temp * 256) + buffer[offset + 1];
      temp = (temp * 256) + buffer[offset + 2];
      temp = (temp * 256) + buffer[offset + 3];

      return (temp);
    }
    public static long Convert4BytesToLong(byte[] buffer, int offset)
    {
      long temp = 0;

      for (int index = 0; index < 4; index++)
        temp = (temp * 256) + buffer[offset + index];

      return (temp);
    }
    public static long Convert8BytesToLong(byte[] buffer, int offset)
    {
      long temp = 0;

      for (int index = 0; index < 8; index++)
        temp = (temp * 256) + buffer[offset + index];

      return (temp);
    }
    public static string ConvertBytesToString(byte[] buffer, int offset, int length)
    {
      StringBuilder reply = new StringBuilder(4);
      for (int index = 0; index < length; index++)
        reply.Append((char)buffer[offset + index]);
      return (reply.ToString());
    }
    public static DateTime NptTimestampToDateTime(long nptTimestamp) { return NptTimestampToDateTime((uint)((nptTimestamp >> 32) & 0xFFFFFFFF), (uint)(nptTimestamp & 0xFFFFFFFF), null); }
    public static DateTime NptTimestampToDateTime(uint seconds, uint fractions, DateTime? epoch)
    {
      ulong ticks = (ulong)((seconds * TimeSpan.TicksPerSecond) + ((fractions * TimeSpan.TicksPerSecond) / 0x100000000L));
      if (epoch.HasValue) return epoch.Value + TimeSpan.FromTicks((Int64)ticks);
      return (seconds & 0x80000000L) == 0 ? UtcEpoch2036 + TimeSpan.FromTicks((Int64)ticks) : UtcEpoch1900 + TimeSpan.FromTicks((Int64)ticks);
    }

    //When the First Epoch will wrap (The real Y2k)
    public static DateTime UtcEpoch2036 = new DateTime(2036, 2, 7, 6, 28, 16, DateTimeKind.Utc);

    public static DateTime UtcEpoch1900 = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime UtcEpoch1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  }
}