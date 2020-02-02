using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Extensions;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ChannelGroupManagement
  {
    public static IList<ChannelGroup> ListAllChannelGroups(ChannelGroupIncludeRelationEnum includeRelations)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ChannelGroups.IncludeAllRelations(includeRelations).ToList();
      }
    }

    public static IList<ChannelGroup> ListAllChannelGroups()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ChannelGroups.IncludeAllRelations().ToList();
      }
    }

    public static IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ChannelGroups.Where(g => g.MediaType == (int)mediaType).IncludeAllRelations().ToList();
      }
    }

    public static IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType, ChannelGroupIncludeRelationEnum includeRelations)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ChannelGroups.Where(g => g.MediaType == (int)mediaType).IncludeAllRelations(includeRelations).ToList();
      }
    }

    public static ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaTypeEnum mediaType)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ChannelGroups.Where(g => g.GroupName == groupName && g.MediaType == (int)mediaType).IncludeAllRelations().FirstOrDefault();
      }
    }

    public static ChannelGroup GetOrCreateGroup(string groupName, MediaTypeEnum mediaType)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        ChannelGroup group = context.ChannelGroups.FirstOrDefault(g => g.GroupName == groupName && g.MediaType == (int)mediaType);
        if (group == null)
        {
          group = new ChannelGroup { GroupName = groupName, SortOrder = 9999, MediaType = (int)mediaType };
          context.Update(group);
          context.SaveChanges(true);
        }
        return group;
      }
    }

    public static void DeleteChannelGroupMap(int idMap)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var group = context.GroupMaps.FirstOrDefault(g => g.GroupMapId == idMap);
        if (group != null)
        {
          context.GroupMaps.Remove(group);
          context.SaveChanges(true);
        }
      }
    }

    public static ChannelGroup GetChannelGroup(int idGroup)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var group = context.ChannelGroups.IncludeAllRelations().FirstOrDefault(g => g.ChannelGroupId == idGroup);
        return group;
      }
    }

    public static ChannelGroup SaveGroup(ChannelGroup group)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(group);
        context.SaveChanges(true);
        return group;
      }
    }

    public static void DeleteChannelGroup(int idGroup)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        // TODO: Check for cascade rule
        var group = context.ChannelGroups.IncludeAllRelations(ChannelGroupIncludeRelationEnum.GroupMaps).FirstOrDefault(g => g.ChannelGroupId == idGroup);
        if (group.GroupMaps.Count > 0)
        {
          foreach (GroupMap groupmap in group.GroupMaps)
          {
            context.GroupMaps.Remove(groupmap);
          }
        }
        context.ChannelGroups.Remove(group);
        context.SaveChanges(true);
      }
    }

    public static IList<ChannelGroup> ListAllCustomChannelGroups(ChannelGroupIncludeRelationEnum includeRelations)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ChannelGroups.IncludeAllRelations(includeRelations)
          .Where(g => g.GroupName != TvConstants.TvGroupNames.AllChannels && g.GroupName != TvConstants.RadioGroupNames.AllChannels)
          .ToList();
      }
    }
  }
}
