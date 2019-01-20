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

using System.Collections.ObjectModel;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Rtcp
{
  public abstract class RtcpPacket
  {
    public int Version { get; private set; }
    public bool Padding { get; private set; }
    public int ReportCount { get; private set; }
    public int Type { get; private set; }
    public int Length { get; private set; }

    public virtual void Parse(byte[] buffer, int offset)
    {
      Version = buffer[offset] >> 6;
      Padding = (buffer[offset] & 0x20) != 0;
      ReportCount = buffer[offset] & 0x1f;
      Type = buffer[offset + 1];
      Length = (Utils.Convert2BytesToInt(buffer, offset + 2) * 4) + 4;
    }
  }

  public class ReportBlock
  {
    /// <summary>
    /// Get the length of the block.
    /// </summary>
    public int BlockLength { get { return (24); } }
    /// <summary>
    /// Get the synchronization source.
    /// </summary>
    public string SynchronizationSource { get; private set; }
    /// <summary>
    /// Get the fraction lost.
    /// </summary>
    public int FractionLost { get; private set; }
    /// <summary>
    /// Get the cumulative packets lost.
    /// </summary>
    public int CumulativePacketsLost { get; private set; }
    /// <summary>
    /// Get the highest number received.
    /// </summary>
    public int HighestNumberReceived { get; private set; }
    /// <summary>
    /// Get the inter arrival jitter.
    /// </summary>
    public int InterArrivalJitter { get; private set; }
    /// <summary>
    /// Get the timestamp of the last report.
    /// </summary>
    public int LastReportTimeStamp { get; private set; }
    /// <summary>
    /// Get the delay since the last report.
    /// </summary>
    public int DelaySinceLastReport { get; private set; }

    /// <summary>
    /// Initialize a new instance of the ReportBlock class.
    /// </summary>
    public ReportBlock() { }

    /// <summary>
    /// Unpack the data in a packet.
    /// </summary>
    /// <param name="buffer">The buffer containing the packet.</param>
    /// <param name="offset">The offset to the first byte of the packet within the buffer.</param>
    /// <returns>An ErrorSpec instance if an error occurs; null otherwise.</returns>
    public void Process(byte[] buffer, int offset)
    {
      SynchronizationSource = Utils.ConvertBytesToString(buffer, offset, 4);
      FractionLost = buffer[offset + 4];
      CumulativePacketsLost = Utils.Convert3BytesToInt(buffer, offset + 5);
      HighestNumberReceived = Utils.Convert4BytesToInt(buffer, offset + 8);
      InterArrivalJitter = Utils.Convert4BytesToInt(buffer, offset + 12);
      LastReportTimeStamp = Utils.Convert4BytesToInt(buffer, offset + 16);
      DelaySinceLastReport = Utils.Convert4BytesToInt(buffer, offset + 20);
    }
  }

  public class RtcpSenderReportPacket : RtcpPacket
  {
    #region Properties
    /// <summary>
    /// Get the synchronization source.
    /// </summary>
    public int SynchronizationSource { get; private set; }
    /// <summary>
    /// Get the NPT timestamp.
    /// </summary>
    public long NPTTimeStamp { get; private set; }
    /// <summary>
    /// Get the RTP timestamp.
    /// </summary>
    public int RTPTimeStamp { get; private set; }
    /// <summary>
    /// Get the packet count.
    /// </summary>
    public int SenderPacketCount { get; private set; }
    /// <summary>
    /// Get the octet count.
    /// </summary>
    public int SenderOctetCount { get; private set; }
    /// <summary>
    /// Get the list of report blocks.
    /// </summary>
    public Collection<ReportBlock> ReportBlocks { get; private set; }
    /// <summary>
    /// Get the profile extension data.
    /// </summary>
    public byte[] ProfileExtension { get; private set; }
    #endregion

    public override void Parse(byte[] buffer, int offset)
    {
      base.Parse(buffer, offset);
      SynchronizationSource = Utils.Convert4BytesToInt(buffer, offset + 4);
      NPTTimeStamp = Utils.Convert8BytesToLong(buffer, offset + 8);
      RTPTimeStamp = Utils.Convert4BytesToInt(buffer, offset + 16);
      SenderPacketCount = Utils.Convert4BytesToInt(buffer, offset + 20);
      SenderOctetCount = Utils.Convert4BytesToInt(buffer, offset + 24);

      ReportBlocks = new Collection<ReportBlock>();
      int index = 28;

      while (ReportBlocks.Count < ReportCount)
      {
        ReportBlock reportBlock = new ReportBlock();
        reportBlock.Process(buffer, offset + index);
        ReportBlocks.Add(reportBlock);
        index += reportBlock.BlockLength;
      }

      if (index < Length)
      {
        ProfileExtension = new byte[Length - index];

        for (int extensionIndex = 0; index < Length; index++)
        {
          ProfileExtension[extensionIndex] = buffer[offset + index];
          extensionIndex++;
        }
      }
    }

    public override string ToString()
    {
      return $@"Sender Report.
Version : {Version}. 
Padding : {Padding}. 
Report Count : {ReportCount}. 
PacketType: {Type}. 
Length : {Length}. 
SynchronizationSource : {SynchronizationSource}.
NTP Timestamp : {Utils.NptTimestampToDateTime(NPTTimeStamp)}.
RTP Timestamp : {RTPTimeStamp}.
Sender Packet Count : {SenderPacketCount}.
Sender Octet : {SenderOctetCount}.";
    }
  }
  public class RtcpReceiverReportPacket : RtcpPacket
  {
    public string SynchronizationSource { get; private set; }
    public Collection<ReportBlock> ReportBlocks { get; private set; }
    public byte[] ProfileExtension { get; private set; }
    public override void Parse(byte[] buffer, int offset)
    {
      base.Parse(buffer, offset);
      SynchronizationSource = Utils.ConvertBytesToString(buffer, offset + 4, 4);

      ReportBlocks = new Collection<ReportBlock>();
      int index = 8;

      while (ReportBlocks.Count < ReportCount)
      {
        ReportBlock reportBlock = new ReportBlock();
        reportBlock.Process(buffer, offset + index);
        ReportBlocks.Add(reportBlock);
        index += reportBlock.BlockLength;
      }

      if (index < Length)
      {
        ProfileExtension = new byte[Length - index];

        for (int extensionIndex = 0; index < Length; index++)
        {
          ProfileExtension[extensionIndex] = buffer[offset + index];
          extensionIndex++;
        }
      }
    }
    public override string ToString()
    {
      return $@"Receiver Report.
Version : {Version}. 
Padding : {Padding}. 
Report Count : {ReportCount}. 
PacketType: {Type}. 
Length : {Length}. 
SynchronizationSource : {SynchronizationSource}.";
    }
  }
  class RtcpSourceDescriptionPacket : RtcpPacket
  { /// <summary>
    /// Get the list of source descriptions.
    /// </summary>
    public Collection<SourceDescriptionBlock> Descriptions;
    public override void Parse(byte[] buffer, int offset)
    {
      base.Parse(buffer, offset);
      Descriptions = new Collection<SourceDescriptionBlock>();

      int index = 4;

      while (Descriptions.Count < ReportCount)
      {
        SourceDescriptionBlock descriptionBlock = new SourceDescriptionBlock();
        descriptionBlock.Process(buffer, offset + index);
        Descriptions.Add(descriptionBlock);
        index += descriptionBlock.BlockLength;
      }
    }
    public override string ToString()
    {
      return $@"Source Description
Version : {Version}. 
Padding : {Padding}. 
Report Count : {ReportCount}. 
PacketType: {Type}. 
Length : {Length}. 
Descriptions : {Descriptions}.";
    }
  }
  public class RtcpByePacket : RtcpPacket
  {
    public Collection<string> SynchronizationSources { get; private set; }
    public string ReasonForLeaving { get; private set; }
    public override void Parse(byte[] buffer, int offset)
    {
      base.Parse(buffer, offset);
      SynchronizationSources = new Collection<string>();
      int index = 4;

      while (SynchronizationSources.Count < ReportCount)
      {
        SynchronizationSources.Add(Utils.ConvertBytesToString(buffer, offset + index, 4));
        index += 4;
      }

      if (index < Length)
      {
        int reasonLength = buffer[offset + index];
        ReasonForLeaving = Utils.ConvertBytesToString(buffer, offset + index + 1, reasonLength);
      }
    }
    public override string ToString()
    {
      return $@"ByeBye
Version : {Version}. 
Padding : {Padding}. 
Report Count : {ReportCount}. 
PacketType: {Type}. 
Length : {Length}. 
SynchronizationSources : {SynchronizationSources}. 
ReasonForLeaving : {ReasonForLeaving}.";
    }
  }
  class RtcpAppPacket : RtcpPacket
  {
    /// <summary>
    /// Get the synchronization source.
    /// </summary>
    public int SynchronizationSource { get; private set; }
    /// <summary>
    /// Get the name.
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// Get the identity.
    /// </summary>
    public int Identity { get; private set; }
    /// <summary>
    /// Get the variable data portion.
    /// </summary>
    public string Data { get; private set; }

    public override void Parse(byte[] buffer, int offset)
    {
      base.Parse(buffer, offset);
      SynchronizationSource = Utils.Convert4BytesToInt(buffer, offset + 4);
      Name = Utils.ConvertBytesToString(buffer, offset + 8, 4);
      Identity = Utils.Convert2BytesToInt(buffer, offset + 12);

      int dataLength = Utils.Convert2BytesToInt(buffer, offset + 14);
      if (dataLength != 0)
        Data = Utils.ConvertBytesToString(buffer, offset + 16, dataLength);
    }
    public override string ToString()
    {
      return $@"Application Specific
Version : {Version}. 
Padding : {Padding}. 
Report Count : {ReportCount}. 
PacketType: {Type}. 
Length : {Length}. 
SynchronizationSource : {SynchronizationSource}. 
Name : {Name}. 
Identity : {Identity}. 
Data : {Data}.";
    }
  }
  class SourceDescriptionBlock
  {
    /// <summary>
    /// Get the length of the block.
    /// </summary>
    public int BlockLength { get { return (blockLength + (blockLength % 4)); } }

    /// <summary>
    /// Get the synchronization source.
    /// </summary>
    public string SynchronizationSource { get; private set; }
    /// <summary>
    /// Get the list of source descriptioni items.
    /// </summary>
    public Collection<SourceDescriptionItem> Items;

    private int blockLength;

    public void Process(byte[] buffer, int offset)
    {
      SynchronizationSource = Utils.ConvertBytesToString(buffer, offset, 4);
      Items = new Collection<SourceDescriptionItem>();
      int index = 4;
      bool done = false;
      do
      {
        SourceDescriptionItem item = new SourceDescriptionItem();
        item.Process(buffer, offset + index);

        if (item.Type != 0)
        {
          Items.Add(item);
          index += item.ItemLength;
          blockLength += item.ItemLength;
        }
        else
        {
          blockLength++;
          done = true;
        }
      }
      while (!done);
    }
  }

  /// <summary>
  /// The class that describes a source description item.
  /// </summary>
  public class SourceDescriptionItem
  {
    /// <summary>
    /// Get the type.
    /// </summary>
    public int Type { get; private set; }
    /// <summary>
    /// Get the text.
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    /// Get the length of the item.
    /// </summary>
    public int ItemLength { get { return (Text.Length + 2); } }

    /// <summary>
    /// Initialize a new instance of the SourceDescriptionItem class.
    /// </summary>
    public SourceDescriptionItem() { }

    /// <summary>
    /// Unpack the data in a packet.
    /// </summary>
    /// <param name="buffer">The buffer containing the packet.</param>
    /// <param name="offset">The offset to the first byte of the packet within the buffer.</param>
    /// <returns>An ErrorSpec instance if an error occurs; null otherwise.</returns>
    public void Process(byte[] buffer, int offset)
    {
      Type = buffer[offset];
      if (Type != 0)
      {
        int length = buffer[offset + 1];
        Text = Utils.ConvertBytesToString(buffer, offset + 2, length);
      }
    }
  }
}
