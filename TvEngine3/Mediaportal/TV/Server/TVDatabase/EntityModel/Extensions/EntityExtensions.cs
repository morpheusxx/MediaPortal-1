using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Extensions
{
  public static class EntityExtensions
  {
    #region Program query extensions

    public static IQueryable<Program> IncludeAllRelations(this IQueryable<Program> query)
    {
      return query
        .Include(p => p.ProgramCategory)
        .Include(p => p.Channel);
    }
    public static IQueryable<Program> GetProgramsByCategory(this IQueryable<Program> query, string searchCriteria, StringComparisonEnum stringComparison)
    {
      DateTime now = DateTime.Now;
      query = query.Where(p => p.Channel.VisibleInGuide && p.EndTime > now);

      if (!string.IsNullOrEmpty(searchCriteria))
      {
        bool startsWith = (stringComparison.HasFlag(StringComparisonEnum.StartsWith));
        bool endsWith = (stringComparison.HasFlag(StringComparisonEnum.EndsWith));

        if (startsWith && endsWith)
        {
          query = query.Where(p => p.ProgramCategory.Category.Contains(searchCriteria));
        }
        else if (!startsWith && !endsWith)
        {
          query = query.Where(p => p.ProgramCategory.Category == searchCriteria);
        }
        else if (startsWith)
        {
          query = query.Where(p => p.ProgramCategory.Category.StartsWith(searchCriteria));
        }
        else
        {
          query = query.Where(p => p.ProgramCategory.Category.EndsWith(searchCriteria));
        }
      }
      return query.OrderBy(p => p.Title).ThenBy(p => p.StartTime);
    }

    public static IQueryable<Program> GetProgramsByTitle(this IQueryable<Program> query, string searchCriteria, StringComparisonEnum stringComparison)
    {
      DateTime now = DateTime.Now;
      query = query.Where(p => p.Channel.VisibleInGuide && p.EndTime > now);

      if (!string.IsNullOrEmpty(searchCriteria))
      {
        bool startsWith = (stringComparison.HasFlag(StringComparisonEnum.StartsWith));
        bool endsWith = (stringComparison.HasFlag(StringComparisonEnum.EndsWith));

        if (startsWith && endsWith)
        {
          query = query.Where(p => p.Title.Contains(searchCriteria));
        }
        else if (!startsWith && !endsWith)
        {
          query = query.Where(p => p.Title == searchCriteria);
        }
        else if (startsWith)
        {
          if (searchCriteria == "[0-9]")
          {
            query = query.Where(p => p.Title.StartsWith("0") || p.Title.StartsWith("1") || p.Title.StartsWith("2") || p.Title.StartsWith("3") || p.Title.StartsWith("4")
                                     || p.Title.StartsWith("5") || p.Title.StartsWith("6") || p.Title.StartsWith("7") || p.Title.StartsWith("8") || p.Title.StartsWith("9"));
          }
          else
          {
            query = query.Where(p => p.Title.StartsWith(searchCriteria));
          }
        }
        else
        {
          query = query.Where(p => p.Title.EndsWith(searchCriteria));
        }
      }
      return query.OrderBy(p => p.Title).ThenBy(p => p.StartTime);
    }

    public static Expression<Func<TElement, bool>> BuildContainsExpression<TElement, TValue>(this IEnumerable<TValue> values, Expression<Func<TElement, TValue>> valueSelector)
    {
      if (null == valueSelector) { throw new ArgumentNullException("valueSelector"); }
      if (null == values) { throw new ArgumentNullException("values"); }
      ParameterExpression p = valueSelector.Parameters.Single();
      // p => valueSelector(p) == values[0] || valueSelector(p) == ...
      if (!values.Any())
      {
        return e => false;
      }
      var equals = values.Select(value => (Expression)Expression.Equal(valueSelector.Body, Expression.Constant(value, typeof(TValue))));
      var body = @equals.Aggregate(Expression.Or);
      return Expression.Lambda<Func<TElement, bool>>(body, p);
    }

    public static IQueryable<Program> GetProgramsByStartEndTimes(this IQueryable<Program> query, DateTime startTime, DateTime endTime)
    {
      return query.Where(p => p.Channel.VisibleInGuide && p.StartTime < endTime && p.EndTime > startTime);
    }

    public static IQueryable<Program> GetNowAndNextProgramsForChannel(this IQueryable<Program> query, int idChannel)
    {
      DateTime now = DateTime.Now;
      var programs =
        query.Where(p => p.ChannelId == idChannel && p.EndTime >= now)
          .Include(p => p.Channel)
          .Include(p => p.ProgramCategory)
          .Include(p => p.ProgramCredits)
          .OrderBy(p => p.StartTime)
          .Take(2);
      return programs;
    }

    public static IQueryable<Program> GetProgramsByDescription(this IQueryable<Program> query, string searchCriteria, StringComparisonEnum stringComparison)
    {
      DateTime now = DateTime.Now;
      query = query.Where(p => p.Channel.VisibleInGuide && p.EndTime > now);

      if (!string.IsNullOrEmpty(searchCriteria))
      {
        bool startsWith = (stringComparison.HasFlag(StringComparisonEnum.StartsWith));
        bool endsWith = (stringComparison.HasFlag(StringComparisonEnum.EndsWith));

        if (startsWith && endsWith)
        {
          query = query.Where(p => p.Description.Contains(searchCriteria));
        }
        else if (!startsWith && !endsWith)
        {
          query = query.Where(p => p.Description == searchCriteria);
        }
        else if (startsWith)
        {
          query = query.Where(p => p.Description.StartsWith(searchCriteria));
        }
        else
        {
          query = query.Where(p => p.Description.EndsWith(searchCriteria));
        }
      }

      return query.OrderBy(p => p.Description).ThenBy(p => p.StartTime);
    }

    public static IQueryable<Program> GetProgramsByTimesInterval(this IQueryable<Program> query, DateTime startTime, DateTime endTime)
    {
      var programsByTimesInterval = query.Where(p => p.Channel.VisibleInGuide &&
                                                                   (p.EndTime > startTime && p.EndTime < endTime)
                                                                   || (p.StartTime >= startTime && p.StartTime <= endTime)
                                                                   || (p.StartTime <= startTime && p.EndTime >= endTime)
        ).OrderBy(p => p.StartTime)
        .Include(p => p.ProgramCategory).Include(p => p.Channel);
      return programsByTimesInterval;
    }

    public static IQueryable<Program> FindAllProgramsByChannelId(this IQueryable<Program> query, int idChannel)
    {
      return query.Where(p => p.ChannelId == idChannel);
    }

    public static IQueryable<Program> AddWeekdayConstraint(this IQueryable<Program> query, DayOfWeek dayOfWeek)
    {
      // TODO workaround/hack for Entity Framework not currently (as of 21-11-2011) supporting DayOfWeek and other similar date functions.
      // once this gets sorted, if ever, this code should be refactored. The db helper fields should be deleted as they have no purpose.
      #region hack
      query = query.Where(p => p.StartTimeDayOfWeek == (int)dayOfWeek);
      #endregion

      //query = query.Where(p => p.startTime.DayOfWeek == dayOfWeek);
      return query;
    }

    /// <summary>
    /// Selects a matching comparision based on the given <paramref name="search"/>. If it contains a SQL wildcard '%', then the EF core "Like" method is used.
    /// Otherwise an equality comparision is used.
    /// </summary>
    /// <param name="query">Programs query</param>
    /// <param name="search">Search term</param>
    /// <returns><c>true</c> if matched</returns>
    public static IQueryable<Program> SmartCompare(this IQueryable<Program> query, string search)
    {
      query = search.Contains("%") ?
        query.Where(p => EF.Functions.Like(p.Title, search)) : 
        query.Where(p => p.Title == search);
      return query;
    }

    #endregion

    #region ProgramCategory query extensions

    public static IQueryable<ProgramCategory> IncludeAllRelations(this IQueryable<ProgramCategory> query)
    {
      var includeRelations = query.Include(p => p.TvGuideCategory);
      return includeRelations;
    }

    #endregion

    #region Channel query extensions

    public static IQueryable<Channel> IncludeAllRelations(this IQueryable<Channel> query)
    {
      ChannelIncludeRelationEnum include = GetAllRelationsForChannel();
      return IncludeAllRelations(query, include);
    }

    public static ChannelIncludeRelationEnum GetAllRelationsForChannel()
    {
      ChannelIncludeRelationEnum include = ChannelIncludeRelationEnum.TuningDetails;
      include |= ChannelIncludeRelationEnum.ChannelMapsCard;
      include |= ChannelIncludeRelationEnum.GroupMaps;
      include |= ChannelIncludeRelationEnum.GroupMapsChannelGroup;
      include |= ChannelIncludeRelationEnum.ChannelMaps;
      include |= ChannelIncludeRelationEnum.ChannelLinkMapsChannelLink;
      include |= ChannelIncludeRelationEnum.ChannelLinkMapsChannelPortal;
      return include;
    }

    public static IQueryable<Channel> IncludeAllRelations(this IQueryable<Channel> query, ChannelIncludeRelationEnum includeRelations)
    {
      bool channelLinkMapsChannelLink = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelLinkMapsChannelLink);
      bool channelLinkMapsChannelPortal = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelLinkMapsChannelPortal);
      bool channelMaps = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelMaps);
      bool groupMaps = includeRelations.HasFlag(ChannelIncludeRelationEnum.GroupMaps);
      bool groupMapsGroup = includeRelations.HasFlag(ChannelIncludeRelationEnum.GroupMapsChannelGroup);
      bool tuningDetails = includeRelations.HasFlag(ChannelIncludeRelationEnum.TuningDetails);
      bool recordings = includeRelations.HasFlag(ChannelIncludeRelationEnum.Recordings);

      if (recordings)
      {
        query = query.Include(c => c.Recordings);
      }

      //todo: move to LoadNavigationProperties for performance improvement
      if (channelLinkMapsChannelLink)
      {
        query = query.Include(c => c.ChannelLinkMaps).ThenInclude(l => l.LinkedChannel);
      }

      //todo: move to LoadNavigationProperties for performance improvement
      if (channelLinkMapsChannelPortal)
      {
        query = query.Include(c => c.ChannelLinkMaps).ThenInclude(l => l.PortalChannel);
      }

      if (channelMaps)
      {
        query = query.Include(c => c.ChannelMaps);
      }

      //too slow, handle in LoadNavigationProperties instead
      //if (channelMapsCard)
      //{      
      //query = query.Include(c => c.ChannelMaps.Select(card => card.Card));
      //}
      if (groupMaps)
      {
        if (groupMapsGroup)
          query = query.Include(c => c.GroupMaps).ThenInclude(m => m.ChannelGroup);
        else
          query = query.Include(c => c.GroupMaps);
      }

      //too slow, handle in LoadNavigationProperties instead
      //if (groupMapsChannelGroup)
      //{
      //  query = query.Include(c => c.GroupMaps.Select(g => g.ChannelGroup));
      //}

      if (tuningDetails)
      {
        query = query.Include(c => c.TuningDetails).ThenInclude(t => t.LnbType);
      }

      return query;
    }

    public static IQueryable<Channel> GetAllChannelsByGroupIdAndMediaType(this IQueryable<GroupMap> query, int groupId, MediaTypeEnum mediaType)
    {
      IQueryable<Channel> channels = query
        .Where(gm => gm.ChannelGroupId == groupId && gm.Channel.VisibleInGuide && gm.Channel.MediaType == (int)mediaType)
        .OrderBy(gm => gm.SortOrder)
        .Select(gm => gm.Channel);
      return channels;
    }

    public static IQueryable<Channel> GetAllChannelsByGroupId(this IQueryable<Channel> query, int groupId)
    {
      IOrderedQueryable<Channel> channels = query
        .Include(c => c.GroupMaps)
        .Where(c => c.VisibleInGuide && c.GroupMaps.Any(gm => gm.ChannelGroup.ChannelGroupId == groupId))
        .OrderBy(c => c.SortOrder);
      return channels;
    }

    public static IQueryable<ChannelMap> IncludeAllRelations(this IQueryable<ChannelMap> query)
    {
      IQueryable<ChannelMap> includeRelations =
        query.
          Include(c => c.Channel).
          Include(c => c.Card);

      return includeRelations;
    }

    #endregion

    #region TuningDetails query extensions

    public static IQueryable<TuningDetail> IncludeAllRelations(this IQueryable<TuningDetail> query)
    {
      IQueryable<TuningDetail> includeRelations = query
        .Include(t => t.LnbType)
        .Include(c => c.Channel)
        .ThenInclude(c => c.GroupMaps);
      return includeRelations;
    }

    #endregion

    #region ChannelGroup query extensions

    public static IQueryable<ChannelGroup> IncludeAllRelations(this IQueryable<ChannelGroup> query)
    {
      var includeRelations = query.
        Include(r => r.GroupMaps).ThenInclude(c => c.Channel.TuningDetails).
        Include(r => r.KeywordMap).
        Include(r => r.GroupMaps).ThenInclude(c => c.Channel);
      return includeRelations;
    }

    public static IQueryable<ChannelGroup> IncludeAllRelations(this IQueryable<ChannelGroup> query, ChannelGroupIncludeRelationEnum includeRelations)
    {
      bool groupMaps = includeRelations.HasFlag(ChannelGroupIncludeRelationEnum.GroupMaps);
      bool groupMapsChannel = includeRelations.HasFlag(ChannelGroupIncludeRelationEnum.GroupMapsChannel);
      bool groupMapsTuningDetails = includeRelations.HasFlag(ChannelGroupIncludeRelationEnum.GroupMapsTuningDetails);
      bool keywordMap = includeRelations.HasFlag(ChannelGroupIncludeRelationEnum.KeywordMap);

      if (groupMaps)
      {
        query = query.Include(r => r.GroupMaps);
      }

      if (keywordMap)
      {
        query = query.Include(r => r.KeywordMap);
      }

      if (groupMapsChannel)
      {
        query = query.Include(r => r.GroupMaps).ThenInclude(c => c.Channel);
      }

      if (groupMapsTuningDetails)
      {
        query = query.Include(r => r.GroupMaps).ThenInclude(c => c.Channel.TuningDetails);
      }

      return query;
    }

    #endregion

    #region Card query extensions

    public static IQueryable<Card> IncludeAllRelations(this IQueryable<Card> query)
    {
      IQueryable<Card> includeRelations =
        query
          .Include(c => c.ChannelMaps).ThenInclude(m => m.Channel).ThenInclude(ch => ch.TuningDetails)
          .Include(c => c.CardGroupMaps)
          .Include(c => c.DisEqcMotors);
      return includeRelations;
    }

    public static IQueryable<Card> IncludeAllRelations(this IQueryable<Card> query, CardIncludeRelationEnum includeRelations)
    {
      bool cardGroupMaps = includeRelations.HasFlag(CardIncludeRelationEnum.CardGroupMaps);
      bool channelMaps = includeRelations.HasFlag(CardIncludeRelationEnum.ChannelMaps);
      bool channelMapsChannelTuningDetails = includeRelations.HasFlag(CardIncludeRelationEnum.ChannelMapsChannelTuningDetails);
      bool disEqcMotors = includeRelations.HasFlag(CardIncludeRelationEnum.DisEqcMotors);

      if (cardGroupMaps)
      {
        query = query.Include(c => c.CardGroupMaps);
      }
      if (channelMaps)
      {
        query = query.Include(c => c.ChannelMaps);
      }
      if (channelMapsChannelTuningDetails)
      {
        query = query.Include(c => c.ChannelMaps).ThenInclude(m => m.Channel).ThenInclude(ch => ch.TuningDetails);
      }
      if (disEqcMotors)
      {
        query = query.Include(c => c.DisEqcMotors);
      }
      return query;
    }

    #endregion

    #region Schedule query extensions

    public static IQueryable<Schedule> IncludeAllRelations(this IQueryable<Schedule> query)
    {
      IQueryable<Schedule> includeRelations = query
        .Include(s => s.Channel).ThenInclude(c => c.TuningDetails)
        .Include(s => s.Recordings)
        .Include(s => s.SubSchedules)
        .Include(s => s.Conflicts)
        .Include(s => s.ParentSchedule)
        .Include(s => s.Channel)
        .Include(s => s.CanceledSchedules);
      return includeRelations;
    }

    public static IQueryable<Schedule> IncludeAllRelations(this IQueryable<Schedule> query, ScheduleIncludeRelationEnum includeRelations)
    {
      bool channel = includeRelations.HasFlag(ScheduleIncludeRelationEnum.Channel);
      bool channelTuningDetails = includeRelations.HasFlag(ScheduleIncludeRelationEnum.ChannelTuningDetails);
      bool conflictingSchedules = includeRelations.HasFlag(ScheduleIncludeRelationEnum.ConflictingSchedules);
      bool conflicts = includeRelations.HasFlag(ScheduleIncludeRelationEnum.Conflicts);
      bool parentSchedule = includeRelations.HasFlag(ScheduleIncludeRelationEnum.ParentSchedule);
      bool recordings = includeRelations.HasFlag(ScheduleIncludeRelationEnum.Recordings);
      bool schedules = includeRelations.HasFlag(ScheduleIncludeRelationEnum.Schedules);
      bool canceledSchedules = includeRelations.HasFlag(ScheduleIncludeRelationEnum.CanceledSchedules);

      if (channel)
      {
        query = query.Include(s => s.Channel);
      }

      if (channelTuningDetails)
      {
        query = query.Include(s => s.Channel.TuningDetails);
      }

      if (conflicts)
      {
        query = query.Include(s => s.Conflicts);
      }

      if (parentSchedule)
      {
        query = query.Include(s => s.ParentSchedule);
      }

      if (recordings)
      {
        query = query.Include(s => s.Recordings);
      }

      if (schedules)
      {
        query = query.Include(s => s.SubSchedules);
      }

      if (canceledSchedules)
      {
        query = query.Include(s => s.CanceledSchedules);
      }

      return query;
    }

    #endregion

    #region Recordings query extensions

    public static IQueryable<Recording> ListAllRecordingsByMediaType(this IQueryable<Recording> query, MediaTypeEnum mediaType)
    {
      IQueryable<Recording> recordings = query.Where(r => r.MediaType == (int)mediaType)
        .Include(r => r.Channel)
        .Include(r => r.RecordingCredits)
        .Include(c => c.Schedule)
        .Include(r => r.ProgramCategory);
      return recordings;
    }

    public static IQueryable<Recording> IncludeAllRelations(this IQueryable<Recording> query)
    {
      var includeRelations = query.Include(r => r.Channel)
        .Include(r => r.RecordingCredits)
        .Include(c => c.Schedule)
        .Include(r => r.ProgramCategory);
      return includeRelations;
    }

    #endregion
  }
}
