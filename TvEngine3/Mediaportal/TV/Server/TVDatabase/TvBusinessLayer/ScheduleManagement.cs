using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Extensions;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Microsoft.EntityFrameworkCore.Internal;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ScheduleManagement
  {

    public static IList<Schedule> ListAllSchedules(ScheduleIncludeRelationEnum includeRelations)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Schedules.IncludeAllRelations().ToList();
      }
    }

    public static IList<Schedule> ListAllSchedules()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Schedules.IncludeAllRelations().ToList();
      }
    }

    public static Schedule SaveSchedule(Schedule schedule)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(schedule);
        context.SaveChanges(true);
      }
      ProgramManagement.SynchProgramStates(schedule.ScheduleId);
      return schedule;
    }

    public static Schedule GetSchedule(int idSchedule)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Schedules.IncludeAllRelations().FirstOrDefault(s => s.ScheduleId == idSchedule);
      }
    }

    public static Schedule GetSchedule(int idSchedule, ScheduleIncludeRelationEnum includeRelations)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Schedules.IncludeAllRelations(includeRelations).FirstOrDefault(s => s.ScheduleId == idSchedule);
      }
    }

    public static bool IsScheduleRecording(int idSchedule)
    {
      bool isScheduleRecording = false;
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        Schedule schedule = context.Schedules.FirstOrDefault(s => s.ScheduleId == idSchedule);

        if (schedule != null)
        {
          Schedule spawnedSchedule = RetrieveSpawnedSchedule(idSchedule, schedule.StartTime);
          if (spawnedSchedule != null)
          {
            schedule = spawnedSchedule;
          }
          isScheduleRecording = (RecordingManagement.GetActiveRecording(schedule.ScheduleId) != null);
        }
      }
      return isScheduleRecording;
    }

    public static Schedule RetrieveSpawnedSchedule(int parentScheduleId, DateTime startTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        Schedule schedule = context.Schedules.FirstOrDefault(s => s.ParentScheduleId == parentScheduleId && s.StartTime == startTime);
        if (schedule == null)
        {
          schedule = context.Schedules.FirstOrDefault(s => s.ParentScheduleId == parentScheduleId);
        }
        return schedule;
      }
    }

    /// <summary>
    /// Retrieves the first found instance of a 'Series' typed schedule given its Channel,Title
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>
    /// <param name="programName">Title we wanna look for</param>    
    /// <returns>schedule instance or null</returns>
    public static Schedule RetrieveSeries(int idChannel, string programName)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var retrieveSeries = context.Schedules.FirstOrDefault(
          s => s.ScheduleType != 0 && s.ChannelId == idChannel && s.ProgramName == programName);
        return retrieveSeries;
      }
    }

    /// <summary>
    /// Retrieves the first found instance of a 'Series' typed schedule given its Channel,Title,Start and End Times 
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>
    /// <param name="programName">Title we wanna look for</param>
    /// <param name="startTime">StartTime</param>
    /// <param name="endTime">EndTime</param>
    /// <returns>schedule instance or null</returns>
    public static Schedule RetrieveSeries(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var retrieveSeries = context.Schedules.FirstOrDefault(
          s =>
          s.ScheduleType != 0 && s.ChannelId == idChannel && s.ProgramName == programName && s.StartTime == startTime &&
          s.EndTime == endTime);
        return retrieveSeries;
      }
    }

    /// <summary>
    /// Retrieves the first found instance of a 'Series' typed schedule given its Channel,Title,Start and End Times 
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>    
    /// <param name="startTime">StartTime</param>
    /// <param name="endTime">EndTime</param>
    /// <returns>schedule instance or null</returns>
    public static Schedule RetrieveSeries(int idChannel, DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var retrieveSeries = context.Schedules.FirstOrDefault(
          s => s.ScheduleType != 0 && s.ChannelId == idChannel && s.StartTime == startTime && s.EndTime == endTime);
        return retrieveSeries;
      }
    }

    public static void DeleteSchedule(int idSchedule)
    {
      Schedule scheduleToDelete;
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        //SetRelatedRecordingsToNull(idSchedule, scheduleRepository);
        scheduleToDelete = context.Schedules.FirstOrDefault(schedule => schedule.ScheduleId == idSchedule);
        if (scheduleToDelete == null)
          return;

        context.Schedules.Remove(scheduleToDelete);
        context.SaveChanges(true);
      }
      ProgramManagement.SynchProgramStates(new ScheduleBLL(scheduleToDelete));
    }

    //private static void SetRelatedRecordingsToNull(int idSchedule, IScheduleRepository scheduleRepository)
    //{
    //  // todo : since "on delete: set null" is not currently supported in EF, we have to do this manually - remove this ugly workaround once EF gets mature enough.
    //  var schedules = scheduleRepository.GetQuery<Schedule>(s => s.ScheduleId == idSchedule);
    //  // Morpheus_xx, 2013-12-15: only include the recordings here, it's the only relation we change. Limiting the query to recordings fixes a bug with SQLite database,
    //  // EF throws an exception: System.ServiceModel.FaultException`1[System.ServiceModel.ExceptionDetail]:
    //  // The type of the key field 'ScheduleId' is expected to be 'System.Int32', but the value provided is actually of type 'System.Int64'.

    //  schedules = scheduleRepository.IncludeAllRelations(schedules, ScheduleIncludeRelationEnum.Recordings);
    //  Schedule schedule = schedules.FirstOrDefault();

    //  if (schedule != null)
    //  {
    //    //scheduleRepository.DeleteList(schedule.Recordings);
    //    for (int i = schedule.Recordings.Count - 1; i >= 0; i--)
    //    {
    //      Recording recording = schedule.Recordings[i];
    //      recording.ScheduleId = null;
    //    }
    //    scheduleRepository.ApplyChanges<Schedule>(scheduleRepository.ObjectContext.Schedules, schedule);
    //  }
    //}

    public static bool IsScheduleRecording(int idSchedule, int idProgram)
    {
      bool isScheduleRecording = false;
      Program prg = ProgramManagement.GetProgram(idProgram);

      if (prg != null)
      {
        var programBll = new ProgramBLL(prg);
        if (programBll.IsRecording)
        {
          using (TvEngineDbContext context = new TvEngineDbContext())
          {
            Schedule schedule = context.Schedules.FirstOrDefault(s => s.ScheduleId == idSchedule);
            if (schedule != null)
            {
              Schedule spawnedSchedule = RetrieveSpawnedSchedule(idSchedule, prg.StartTime);
              if (spawnedSchedule != null)
              {
                schedule = spawnedSchedule;
              }
            }
            if (schedule != null)
            {
              isScheduleRecording = (RecordingManagement.GetActiveRecording(schedule.ScheduleId) != null);
            }
          }
        }
      }

      return isScheduleRecording;
    }

    public static Schedule GetScheduleWithNoEPG(int idChannel)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var scheduleWithNoEpg = context.Schedules.FirstOrDefault(
          s => s.ScheduleType == 0 && s.ChannelId == idChannel && s.ParentScheduleId <= 0 && !s.Series);
        return scheduleWithNoEpg;
      }
    }

    public static void UnCancelSerie(Schedule schedule, DateTime startTime, int idChannel)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        foreach (CanceledSchedule canceledSchedule in schedule.CanceledSchedules)
        {
          if (canceledSchedule.CancelDateTime == startTime && canceledSchedule.ChannelId == idChannel)
          {
            context.CanceledSchedules.Remove(canceledSchedule);
            ProgramManagement.SetSingleStateSeriesPending(canceledSchedule.CancelDateTime,
                                                           canceledSchedule.ChannelId,
                                                           canceledSchedule.Schedule.ProgramName);
          }
        }
        context.SaveChanges(true);
      }
    }


    public static IList<Schedule> GetConflictingSchedules(Schedule sched, out List<Schedule> notViewableSchedules)
    {
      notViewableSchedules = new List<Schedule>();
      sched.Channel = ChannelManagement.GetChannel(sched.ChannelId);
      Log.Info("GetConflictingSchedules: Schedule = " + sched);
      var conflicts = new List<Schedule>();
      IEnumerable<Schedule> schedulesList = ListAllSchedules();

      IList<Card> cards = CardManagement.ListAllCards(CardIncludeRelationEnum.None).ToList(); //SEB
      if (cards.Count == 0)
      {
        return conflicts;
      }
      Log.Info("GetConflictingSchedules: Cards.Count = {0}", cards.Count);

      List<Schedule>[] cardSchedules = new List<Schedule>[cards.Count];
      for (int i = 0; i < cards.Count; i++)
      {
        cardSchedules[i] = new List<Schedule>();
      }

      // GEMX: Assign all already scheduled timers to cards. Assume that even possibly overlapping schedulues are ok to the user,
      // as he decided to keep them before. That's why they are in the db
      foreach (Schedule schedule in schedulesList)
      {


        List<Schedule> episodes = GetRecordingTimes(schedule);
        foreach (Schedule episode in episodes)
        {
          if (DateTime.Now > episode.EndTime)
          {
            continue;
          }
          var episodeBLL = new ScheduleBLL(episode);
          if (episodeBLL.IsSerieIsCanceled(episode.StartTime))
          {
            continue;
          }
          IList<Schedule> overlapping;
          AssignSchedulesToCard(episode, cardSchedules, out overlapping, out notViewableSchedules);
        }
      }

      List<Schedule> newEpisodes = GetRecordingTimes(sched);
      foreach (Schedule newEpisode in newEpisodes)
      {
        if (DateTime.Now > newEpisode.EndTime)
        {
          continue;
        }
        var newEpisodeBLL = new ScheduleBLL(newEpisode);
        if (newEpisodeBLL.IsSerieIsCanceled(newEpisode.StartTime))
        {
          continue;
        }
        IList<Schedule> overlapping;
        List<Schedule> notViewable;
        if (!AssignSchedulesToCard(newEpisode, cardSchedules, out overlapping, out notViewable))
        {
          Log.Info("GetConflictingSchedules: newEpisode can not be assigned to a card = " + newEpisode);
          conflicts.AddRange(overlapping);
          notViewableSchedules.AddRange(notViewable);
        }
      }
      return conflicts;
    }

    private static bool AssignSchedulesToCard(Schedule schedule, List<Schedule>[] cardSchedules, out IList<Schedule> overlappingSchedules, out List<Schedule> notViewabledSchedules)
    {
      overlappingSchedules = new List<Schedule>();
      notViewabledSchedules = new List<Schedule>();
      Log.Info("AssignSchedulesToCard: schedule = " + schedule);
      IEnumerable<Card> cards = CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB
      bool assigned = false;
      bool canView = false;
      int count = 0;
      foreach (Card card in cards)
      {
        ScheduleBLL scheduleBll = new ScheduleBLL(schedule);
        if (card.Enabled && CardManagement.CanViewTvChannel(card, schedule.ChannelId))
        {
          canView = true;
          // checks if any schedule assigned to this cards overlaps current parsed schedule
          bool free = true;
          foreach (Schedule assignedSchedule in cardSchedules[count])
          {
            Log.Info("AssignSchedulesToCard: card {0}, ID = {1} has schedule = " + assignedSchedule, count, card.CardId);
            bool hasOverlappingSchedule = scheduleBll.IsOverlapping(assignedSchedule);
            if (hasOverlappingSchedule)
            {
              bool isSameTransponder = (scheduleBll.IsSameTransponder(assignedSchedule));
              if (!isSameTransponder)
              {
                overlappingSchedules.Add(assignedSchedule);
                Log.Info("AssignSchedulesToCard: overlapping with " + assignedSchedule + " on card {0}, ID = {1}", count,
                         card.CardId);
                free = false;
                break;
              }
            }
          }
          if (free)
          {
            Log.Info("AssignSchedulesToCard: free on card {0}, ID = {1}", count, card.CardId);
            cardSchedules[count].Add(schedule);
            assigned = true;
            break;
          }
        }
        count++;
      }
      if (!canView)
      {
        notViewabledSchedules.Add(schedule);
      }
      return (canView && assigned);
    }

    public static List<Schedule> GetRecordingTimes(Schedule rec)
    {
      return GetRecordingTimes(rec, 10);
    }

    public static List<Schedule> GetRecordingTimes(Schedule rec, int days)
    {
      var recordings = new List<Schedule>();
      var recBLL = new ScheduleBLL(rec);

      DateTime dtDay = DateTime.Now;
      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.Once)
      {
        recordings.Add(recBLL.Entity);
        return recordings;
      }

      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.Daily)
      {
        for (int i = 0; i < days; ++i)
        {
          var recNew = ScheduleFactory.Clone(recBLL.Entity);
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.StartTime.Hour, recBLL.Entity.StartTime.Minute,
                                          0);
          if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
          {
            dtDay = dtDay.AddDays(1);
          }
          recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.EndTime.Hour, recBLL.Entity.EndTime.Minute, 0);
          if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
          {
            dtDay = dtDay.AddDays(-1);
          }
          recNew.Series = true;
          if (recNew.StartTime >= DateTime.Now)
          {
            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.WorkingDays)
      {
        for (int i = 0; i < days; ++i)
        {
          if (WeekEndTool.IsWorkingDay(dtDay.DayOfWeek))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.StartTime.Hour, recBLL.Entity.StartTime.Minute,
                                            0);
            if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.EndTime.Hour, recBLL.Entity.EndTime.Minute, 0);
            if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.Weekends)
      {
        IEnumerable<Program> progList = ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(recBLL.Entity.ChannelId,
                                                                        recBLL.Entity.ProgramName, dtDay,
                                                                        dtDay.AddDays(days));
        foreach (Program prog in progList)
        {
          if ((recBLL.IsRecordingProgram(prog, false)) &&
              (WeekEndTool.IsWeekend(prog.StartTime.DayOfWeek)))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = prog.StartTime;
            recNew.EndTime = prog.EndTime;
            recNew.Series = true;

            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
          }
        }
        return recordings;
      }
      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.Weekly)
      {
        for (int i = 0; i < days; ++i)
        {
          if ((dtDay.DayOfWeek == recBLL.Entity.StartTime.DayOfWeek) && (dtDay.Date >= recBLL.Entity.StartTime.Date))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.StartTime.Hour, recBLL.Entity.StartTime.Minute,
                                            0);
            if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.EndTime.Hour, recBLL.Entity.EndTime.Minute, 0);
            if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      IEnumerable<Program> programs;
      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel)
      {
        //this.LogDebug("get {0} {1} EveryTimeOnThisChannel", rec.ProgramName, rec.ReferencedChannel().Name);
        programs = ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(recBLL.Entity.ChannelId,
                                                                        recBLL.Entity.ProgramName, dtDay,
                                                                        dtDay.AddDays(days));
        foreach (Program prog in programs)
        {
          // dtDay.DayOfWeek == rec.startTime.DayOfWeek
          // this.LogDebug("BusinessLayer.cs Program prog in programs WeeklyEveryTimeOnThisChannel: {0} {1} prog.startTime.DayOfWeek == rec.startTime.DayOfWeek {2} == {3}", rec.ProgramName, rec.ReferencedChannel().Name, prog.startTime.DayOfWeek, rec.startTime.DayOfWeek);
          if (prog.StartTime.DayOfWeek == recBLL.Entity.StartTime.DayOfWeek && recBLL.IsRecordingProgram(prog, false))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.ChannelId = prog.ChannelId;
            recNew.StartTime = prog.StartTime;
            recNew.EndTime = prog.EndTime;
            recNew.Series = true;
            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
            //this.LogDebug("BusinessLayer.cs Added Recording WeeklyEveryTimeOnThisChannel: {0} {1} prog.startTime.DayOfWeek == rec.startTime.DayOfWeek {2} == {3}", rec.ProgramName, rec.ReferencedChannel().Name, prog.startTime.DayOfWeek, rec.startTime.DayOfWeek);
          }
        }
        return recordings;
      }

      programs = recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel
                   ? ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(recBLL.Entity.ChannelId,
                                          recBLL.Entity.ProgramName, dtDay,
                                          dtDay.AddDays(days))
                   : ProgramManagement.GetProgramsByTitleAndStartEndTimes(recBLL.Entity.ProgramName, dtDay, dtDay.AddDays(days));

      foreach (Program prog in programs)
      {
        if (recBLL.IsRecordingProgram(prog, false))
        {
          Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.ChannelId = prog.ChannelId;
          recNew.StartTime = prog.StartTime;
          recNew.EndTime = prog.EndTime;
          recNew.Series = true;
          if (recBLL.IsSerieIsCanceled(recBLL.GetSchedStartTimeForProg(prog), prog.ChannelId))
          {
            recNew.Canceled = recNew.StartTime;
          }
          recordings.Add(recNew);
        }
      }
      return recordings;
    }


    public static bool DoesScheduleUseEpisodeManagement(Schedule schedule)
    {
      if (schedule.MaxAirings == Int32.MaxValue)
      {
        return false;
      }
      if (schedule.MaxAirings < 1)
      {
        return false;
      }
      return true;
    }

    public static void DeleteOrphanedOnceSchedules()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        IList<Schedule> schedules = context.Schedules.Where(
            s => s.ScheduleType == (int)ScheduleRecordingType.Once && s.EndTime < DateTime.Now).ToList();

        if (schedules.Count > 0)
        {
          Log.Debug("DeleteOrphanedOnceSchedules: Orphaned once schedule(s) found  - removing");

          //foreach (var schedule in schedules)
          //{
          //  SetRelatedRecordingsToNull(schedule.ScheduleId, scheduleRepository);
          //}
          foreach (Schedule schedule in schedules)
          {
            context.Schedules.Remove(schedule);
          }
          context.SaveChanges(true);
        }
      }
    }

    public static Schedule RetrieveOnce(int idChannel, string title, DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        Schedule onceSchedule = context.Schedules.FirstOrDefault(s => s.ScheduleType == (int)ScheduleRecordingType.Once
                                                                          && s.ChannelId == idChannel && s.ProgramName == title && s.EndTime == endTime);
        return onceSchedule;
      }
    }

    public static IList<ScheduleRulesTemplate> ListAllScheduleRules()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        IQueryable<ScheduleRulesTemplate> listAllScheduleRules = context.ScheduleRulesTemplates;
        //listAllScheduleRules = scheduleRepository.IncludeAllRelations(listAllScheduleRules);
        return listAllScheduleRules.ToList();
      }
    }

    public static IList<ScheduleRulesTemplate> ListAllScheduleRulesTemplates()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        IQueryable<ScheduleRulesTemplate> listAllScheduleRulesTemplates = context.ScheduleRulesTemplates;
        //listAllScheduleRules = scheduleRepository.IncludeAllRelations(listAllScheduleRules);
        return listAllScheduleRulesTemplates.ToList();
      }
    }

    public static RuleBasedSchedule SaveRuleBasedSchedule(RuleBasedSchedule schedule)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(schedule);
        context.SaveChanges(true);
        return schedule;
      }
    }
  }
}
