using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public static class MappingHelper
  {
    public static GroupMap AddChannelToGroup(ref Channel channel, ChannelGroup @group)
    {
      foreach (GroupMap groupMap in channel.GroupMaps.Where(groupMap => groupMap.ChannelGroupId == @group.ChannelGroupId))
      {
        return groupMap;
      }
      DoAddChannelToGroup(channel, group);
      channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(channel);
      //channel.AcceptChanges();
      return channel.GroupMaps.FirstOrDefault(gMap => gMap.ChannelGroupId == @group.ChannelGroupId);
    }

    private static void DoAddChannelToGroup(Channel channel, ChannelGroup @group)
    {
      var groupMap = new GroupMap
      {
        //Channel = channel, // causes : AcceptChanges cannot continue because the object's key values conflict with another object in the ObjectStateManager. Make sure that the key values are unique before calling AcceptChanges.
        //ChannelGroup = group,
        ChannelId = channel.ChannelId,
        ChannelGroupId = @group.ChannelGroupId,
        SortOrder = channel.SortOrder
      };
      channel.GroupMaps.Add(groupMap);
    }

    public static GroupMap AddChannelToGroup(ref Channel channel, string groupName, MediaTypeEnum mediaType)
    {
      ChannelGroup channelGroup = ServiceAgents.Instance.ChannelGroupServiceAgent.GetChannelGroupByNameAndMediaType(groupName, mediaType);
      if (channelGroup != null)
      {
        return AddChannelToGroup(ref channel, channelGroup);
      }
      return null;
    }

    public static void AddChannelsToGroup(IEnumerable<Channel> channels, ChannelGroup group)
    {
      foreach (Channel channel in channels)
      {
        DoAddChannelToGroup(channel, group);
      }
      ServiceAgents.Instance.ChannelServiceAgent.SaveChannels(channels);
    }

    public static ChannelMap AddChannelToCard(Channel channel, Card card, bool epg)
    {
      foreach (ChannelMap channelMap in channel.ChannelMaps.Where(chMap => chMap.CardId == card.CardId))
      {
        //already associated ?
        return channelMap;
      }

      var map = new ChannelMap()
      {
        ChannelId = channel.ChannelId,
        CardId = card.CardId,
        EpgOnly = epg
      };

      channel.ChannelMaps.Add(map);
      channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(channel);
      //channel.AcceptChanges();
      return channel.ChannelMaps.FirstOrDefault(chMap => chMap.CardId == card.CardId);
    }



  }
}
