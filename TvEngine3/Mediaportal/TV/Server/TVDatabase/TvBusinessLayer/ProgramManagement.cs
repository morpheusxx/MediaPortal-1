using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediaportal.Common.Utils;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Extensions;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Microsoft.EntityFrameworkCore;
using ThreadState = System.Threading.ThreadState;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public class ProgramManagement
  {
    private static Thread _insertProgramsThread;
    private static readonly Queue<ImportParams> _programInsertsQueue = new Queue<ImportParams>();
    private static readonly AutoResetEvent _pendingProgramInserts = new AutoResetEvent(false);

    public static IDictionary<int, NowAndNext> GetNowAndNextForChannelGroup(int idGroup)
    {
      Stopwatch s = Stopwatch.StartNew();
      try
      {
        using (TvEngineDbContext context = new TvEngineDbContext())
        {
          //IQueryable<Channel> channels = programRepository.GetQuery<Channel>(c => c.GroupMaps.Any(g => g.idGroup == idGroup));
          //IList<int> channelIds = channels.Select(c => c.idChannel).ToList();

          IDictionary<int, NowAndNext> progList = new Dictionary<int, NowAndNext>();

          IList<Program> nowPrograms;
          IList<Program> nextPrograms = null;

          ThreadHelper.ParallelInvoke(
            delegate
            {
              Stopwatch s1 = Stopwatch.StartNew();
              using (TvEngineDbContext programRepositoryForThread = new TvEngineDbContext())
              {
                IQueryable<Program> nowProgramsForChannelGroup =
                  GetNowProgramsForChannelGroup(programRepositoryForThread, idGroup);
                //this.LogDebug("GetNowProgramsForChannelGroup SQL = {0}", nowProgramsForChannelGroup.ToTraceString());
                nowPrograms = nowProgramsForChannelGroup.ToList();
              }

              Log.Debug("GetNowProgramsForChannelGroup took {0}", s1.ElapsedMilliseconds);
              AddNowProgramsToList(nowPrograms, progList);
            }
            ,
            delegate
            {
              Stopwatch s2 = Stopwatch.StartNew();
              IQueryable<Program> nextProgramsForChannelGroup = GetNextProgramsForChannelGroup(context, idGroup);
              //this.LogDebug("GetNextProgramsForChannelGroup SQL = {0}", nextProgramsForChannelGroup.ToTraceString());
              nextPrograms = nextProgramsForChannelGroup.ToList();
              Log.Debug("GetNowProgramsForChannelGroup took {0}", s2.ElapsedMilliseconds);
            }
          );

          AddNextProgramsToList(nextPrograms, progList);
          return progList;
        }
      }
      catch (Exception ex)
      {
        Log.Error("ProgramManagement.GetNowAndNextProgramsForChannels ex={0}", ex);
        throw;
      }
      finally
      {
        Log.Debug("GetNowAndNextForChannelGroup took {0}", s.ElapsedMilliseconds);
      }
    }

    private static void AddNextProgramsToList(IEnumerable<Program> nextPrograms, IDictionary<int, NowAndNext> progList)
    {
      foreach (Program nextPrg in nextPrograms)
      {
        NowAndNext nowAndNext;
        progList.TryGetValue(nextPrg.ChannelId, out nowAndNext);

        int idChannel = nextPrg.ChannelId;
        string titleNext = nextPrg.Title;
        int idProgramNext = nextPrg.ProgramId;
        string episodeNameNext = nextPrg.EpisodeName;
        string seriesNumNext = nextPrg.SeriesNum;
        string episodeNumNext = nextPrg.EpisodeNum;
        string episodePartNext = nextPrg.EpisodePart;

        if (nowAndNext == null)
        {
          DateTime nowStart = SqlDateTime.MinValue.Value;
          DateTime nowEnd = SqlDateTime.MinValue.Value;
          ;
          string titleNow = string.Empty;
          int idProgramNow = -1;
          string episodeNameNow = string.Empty;
          string seriesNumNow = string.Empty;
          string episodeNumNow = string.Empty;
          string episodePartNow = string.Empty;
          nowAndNext = new NowAndNext(idChannel, nowStart, nowEnd, titleNow, titleNext, idProgramNow,
            idProgramNext, episodeNameNow, episodeNameNext, seriesNumNow,
            seriesNumNext, episodeNumNow, episodeNumNext, episodePartNow,
            episodePartNext);
        }
        else
        {
          nowAndNext.TitleNext = titleNext;
          nowAndNext.ProgramNextId = idProgramNext;
          nowAndNext.EpisodeNameNext = episodeNameNext;
          nowAndNext.SeriesNumNext = seriesNumNext;
          nowAndNext.EpisodeNumNext = episodeNumNext;
          nowAndNext.EpisodePartNext = episodePartNext;
        }

        progList[idChannel] = nowAndNext;
      }
    }

    private static void AddNowProgramsToList(IEnumerable<Program> nowPrograms, IDictionary<int, NowAndNext> progList)
    {
      foreach (Program nowPrg in nowPrograms)
      {
        int idChannel = nowPrg.ChannelId;
        string titleNext = string.Empty;
        int idProgramNext = -1;
        string episodeNameNext = string.Empty;
        string seriesNumNext = string.Empty;
        string episodeNumNext = string.Empty;
        string episodePartNext = string.Empty;

        DateTime nowStart = nowPrg.StartTime;
        DateTime nowEnd = nowPrg.EndTime;
        string titleNow = nowPrg.Title;
        int idProgramNow = nowPrg.ProgramId;
        string episodeNameNow = nowPrg.EpisodeName;
        string seriesNumNow = nowPrg.SeriesNum;
        string episodeNumNow = nowPrg.EpisodeNum;
        string episodePartNow = nowPrg.EpisodePart;

        var nowAndNext = new NowAndNext(idChannel, nowStart, nowEnd, titleNow, titleNext, idProgramNow,
          idProgramNext, episodeNameNow, episodeNameNext, seriesNumNow,
          seriesNumNext, episodeNumNow, episodeNumNext, episodePartNow,
          episodePartNext);
        progList[idChannel] = nowAndNext;
      }
    }


    public void InsertPrograms(ImportParams importParams)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Database.BeginTransaction();
        switch (importParams.ProgamsToDelete)
        {
          case DeleteBeforeImportOption.OverlappingPrograms:
            IEnumerable<ProgramListPartition> partitions = importParams.ProgramList.GetPartitions();
            DeleteProgramsByPartitions(context, partitions);
            break;
          case DeleteBeforeImportOption.ProgramsOnSameChannel:
            IEnumerable<int> channelIds = importParams.ProgramList.GetChannelIds();
            DeleteProgramsByIds(context, channelIds);
            break;
        }

        foreach (Program program in importParams.ProgramList)
        {
          SynchronizeDateHelpers(program);
        }

        context.Programs.AddRange(importParams.ProgramList);
        context.SaveChanges();
        context.Database.CommitTransaction();
      }

      //no need to do a manual transaction rollback on UnitOfWork as it does this internally already in case of exceptions
    }

    private void DeleteProgramsByIds(TvEngineDbContext context, IEnumerable<int> channelIds)
    {
      var programs = context.Programs.Where(t => channelIds.Any(c => c == t.ChannelId));
      if (programs.Any())
        context.Programs.RemoveRange(programs);
    }

    private void DeleteProgramsByPartitions(TvEngineDbContext context,
      IEnumerable<ProgramListPartition> deleteProgramRanges)
    {
      /*sqlCmd.CommandText =
      "DELETE FROM Program WHERE idChannel = @idChannel AND ((endTime > @rangeStart AND startTime < @rangeEnd) OR (startTime = endTime AND startTime BETWEEN @rangeStart AND @rangeEnd))";
      */


      foreach (ProgramListPartition part in deleteProgramRanges)
      {
        ProgramListPartition partition = part;
        var toRemove = context.Programs.Where(t =>
          t.ChannelId == partition.ChannelId && ((t.EndTime > partition.Start && t.StartTime < partition.End)) ||
          (t.StartTime == t.EndTime && t.StartTime >= partition.Start && t.StartTime <= partition.End));
        if (toRemove.Any())
          context.Programs.RemoveRange(toRemove);
      }
    }

    public static void DeleteAllPrograms()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        string sql = "Delete FROM programs";
        context.Database.ExecuteSqlRaw(sql);
        context.SaveChanges();
      }
    }

    public void PersistProgram(Program prg)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Programs.Add(prg);
        context.SaveChanges();
      }
    }

    public IList<Program> FindAllProgramsByChannelId(int idChannel)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var query = context.Programs.IncludeAllRelations().FindAllProgramsByChannelId(idChannel)
          .OrderBy(t => t.StartTime);
        return query.ToList();
      }
    }

    public static IList<Program> GetProgramsByChannelAndStartEndTimes(int idChannel, DateTime startTime,
      DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var query = context.Programs.IncludeAllRelations()
          .GetProgramsByStartEndTimes(startTime, endTime)
          .Where(p => p.ChannelId == idChannel)
          .OrderBy(p => p.StartTime);
        return query.ToList();
      }
    }

    public static IList<Program> GetProgramsByChannelAndTitleAndStartEndTimes(int idChannel, string title,
      DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var query = context.Programs.IncludeAllRelations()
          .GetProgramsByStartEndTimes(startTime, endTime)
          .Where(p => p.ChannelId == idChannel)
          .Where(p => p.Title == title)
          .OrderBy(p => p.StartTime);
        return query.ToList();
      }
    }

    public static IList<Program> GetProgramsByTitleAndStartEndTimes(string title, DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var query = context.Programs.IncludeAllRelations()
          .GetProgramsByStartEndTimes(startTime, endTime)
          .Where(p => p.Title == title)
          .OrderBy(p => p.StartTime);
        return query.ToList();
      }
    }

    public static IList<Program> GetNowAndNextProgramsForChannel(int idChannel)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Programs.GetNowAndNextProgramsForChannel(idChannel).ToList();
      }
    }

    public static Program GetProgramAt(DateTime date, int idChannel)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var programAt = context.Programs
          .IncludeAllRelations()
          .Where(p => p.ChannelId == idChannel && p.EndTime > date && p.StartTime <= date)
          .OrderBy(p => p.StartTime)
          .FirstOrDefault();

        return programAt;
      }
    }

    public void PersistPrograms(IEnumerable<Program> programs)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Programs.AddRange(programs);
        context.SaveChanges();
      }
    }

    /// <summary>
    /// Batch inserts programs - intended for faster EPG import. You must make sure before that there are no duplicates 
    /// (e.g. delete all program data of the current channel).
    /// Also you MUST provide a true copy of "aProgramList". If you update it's reference in your own code the values will get overwritten
    /// (possibly before they are written to disk)!
    /// </summary>
    /// <param name="aProgramList">A list of persistable gentle.NET Program objects mapping to the Programs table</param>
    /// <param name="progamsToDelete">Flag specifying which existing programs to delete before the insert</param>
    /// <param name="aThreadPriority">Use "Lowest" for Background imports allowing LiveTV, AboveNormal for full speed</param>
    /// <returns>The record count of programs if successful, 0 on errors</returns>
    /// <remarks><para>Inserts are queued to be performed in the background. Each batch of inserts is executed in a single transaction.
    /// You may also optionally specify to delete either all existing programs in the same channel(s) as the programs to be inserted 
    /// (<see cref="DeleteBeforeImportOption.ProgramsOnSameChannel"/>), or existing programs that would otherwise overlap new programs
    /// (<see cref="DeleteBeforeImportOption.OverlappingPrograms"/>), or none (<see cref="DeleteBeforeImportOption.None"/>).
    /// The deletion is also performed in the same transaction as the inserts so that EPG will not be at any time empty.</para>
    /// <para>After all insert have completed and the background thread is idle for 60 seconds, the program states are
    /// automatically updated to reflect the changes.</para></remarks>
    public int InsertPrograms(List<Program> aProgramList, DeleteBeforeImportOption progamsToDelete,
      ThreadPriority aThreadPriority)
    {
      try
      {
        int sleepTime = 10;

        switch (aThreadPriority)
        {
          case ThreadPriority.Highest:
          case ThreadPriority.AboveNormal:
            aThreadPriority = ThreadPriority.Normal;
            sleepTime = 0;
            break;
          case ThreadPriority.Normal:
            // this is almost enough on dualcore systems for one cpu to gather epg and the other to insert it
            sleepTime = 10;
            break;
          case ThreadPriority.BelowNormal: // on faster systems this might be enough for background importing
            sleepTime = 20;
            break;
          case ThreadPriority.Lowest: // even a single core system is enough to use MP while importing.
            sleepTime = 40;
            break;
        }

        ImportParams param = new ImportParams();
        param.ProgramList = new ProgramList(aProgramList);
        param.ProgamsToDelete = progamsToDelete;
        param.SleepTime = sleepTime;
        param.Priority = aThreadPriority;

        lock (_programInsertsQueue)
        {
          _programInsertsQueue.Enqueue(param);
          _pendingProgramInserts.Set();

          if (_insertProgramsThread == null)
          {
            _insertProgramsThread = new Thread(InsertProgramsThreadStart)
            {
              Priority = ThreadPriority.Lowest,
              Name = "SQL EPG importer",
              IsBackground = true
            };
            _insertProgramsThread.Start();
          }
        }

        return aProgramList.Count;
      }
      catch (Exception ex)
      {
        this.LogError("BusinessLayer: InsertPrograms error - {0}, {1}", ex.Message, ex.StackTrace);
        return 0;
      }
    }

    public static void InitiateInsertPrograms()
    {
      Thread currentInsertThread = _insertProgramsThread;
      if (currentInsertThread != null && !currentInsertThread.ThreadState.HasFlag(ThreadState.Unstarted))
        currentInsertThread.Join();
    }

    /// <summary>
    /// Batch inserts programs - intended for faster EPG import. You must make sure before that there are no duplicates 
    /// (e.g. delete all program data of the current channel).
    /// Also you MUST provide a true copy of "aProgramList". If you update it's reference in your own code the values will get overwritten
    /// (possibly before they are written to disk)!
    /// </summary>
    /// <param name="aProgramList">A list of persistable gentle.NET Program objects mapping to the Programs table</param>
    /// <param name="aThreadPriority">Use "Lowest" for Background imports allowing LiveTV, AboveNormal for full speed</param>
    /// <returns>The record count of programs if successful, 0 on errors</returns>
    public int InsertPrograms(List<Program> aProgramList, ThreadPriority aThreadPriority)
    {
      return InsertPrograms(aProgramList, DeleteBeforeImportOption.None, aThreadPriority);
    }

    private void InsertProgramsThreadStart()
    {
      try
      {
        this.LogDebug("BusinessLayer: InsertProgramsThread started");
        DateTime lastImport = DateTime.Now;
        while (true)
        {
          if (lastImport.AddSeconds(60) < DateTime.Now)
          {
            // Done importing and 60 seconds since last import
            // Remove old programs

            // Let's update states
            SynchProgramStatesForAllSchedules();
            // and exit
            lock (_programInsertsQueue)
            {
              //  Has new work been queued in the meantime?
              if (_programInsertsQueue.Count == 0)
              {
                this.LogDebug("BusinessLayer: InsertProgramsThread exiting");
                _insertProgramsThread = null;
                break;
              }
            }
          }

          _pendingProgramInserts.WaitOne(10000); // Check every 10 secs
          while (_programInsertsQueue.Count > 0)
          {
            try
            {
              ImportParams importParams;
              lock (_programInsertsQueue)
              {
                importParams = _programInsertsQueue.Dequeue();
              }

              Thread.CurrentThread.Priority = importParams.Priority;
              InsertPrograms(importParams);
              this.LogDebug("BusinessLayer: Inserted {0} programs to the database", importParams.ProgramList.Count);
              lastImport = DateTime.Now;
              Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            }
            catch (Exception ex)
            {
              this.LogError("BusinessLayer: InsertMySQL/InsertMSSQL caused an exception:");
              this.LogError(ex);
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError("BusinessLayer: InsertProgramsThread error - {0}, {1}", ex.Message, ex.StackTrace);
      }
    }

    public static void SynchProgramStatesForAllSchedules()
    {
      Log.Info("SynchProgramStatesForAllSchedules");
      var schedules = ScheduleManagement.ListAllSchedules(ScheduleIncludeRelationEnum.None);
      if (schedules != null)
      {
        Parallel.ForEach(schedules, schedule => SynchProgramStates(schedule.ScheduleId));
      }
    }

    public void InitiateInsertPrograms(int millisecondsTimeout)
    {
      Thread currentInsertThread = _insertProgramsThread;
      if (currentInsertThread != null && !currentInsertThread.ThreadState.HasFlag(ThreadState.Unstarted))
        currentInsertThread.Join(millisecondsTimeout);
    }


    public static Program RetrieveByTitleTimesAndChannel(string programName, DateTime startTime, DateTime endTime,
      int idChannel)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Programs
          .IncludeAllRelations()
          .FirstOrDefault(p =>
            p.Title == programName && p.StartTime == startTime && p.EndTime == endTime && p.ChannelId == idChannel);
      }
    }

    public static IList<Program> RetrieveDaily(DateTime startTime, DateTime endTime, int idChannel)
    {
      return RetrieveDaily(startTime, endTime, idChannel, -1);
    }

    public static IList<Program> RetrieveDaily(DateTime startTime, DateTime endTime, int channelId, int maxDays)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        DateTime now = DateTime.Now;
        var query = context.Programs
          .IncludeAllRelations()
          .Where(p => p.EndTime >= now && p.ChannelId == channelId)
          .AddTimeRangeConstraint(startTime, endTime);

        if (maxDays > 0)
        {
          query = query.Where(p => p.StartTime < now.AddDays(maxDays));
        }

        return query.ToList();
      }
    }

    /*private static IQueryable<Program> AddTimeRangeConstraint(IQueryable<Program> query, DateTime startTime, DateTime endTime)
    {      
      TimeSpan startOffset = startTime.TimeOfDay;
      TimeSpan endOffset = endTime - startTime.Date;
      if (startOffset > endOffset)
      {
        endOffset = endOffset.Add(new TimeSpan(1,0,0,0)); //AddDays(1);
      }
      query = query.Where(p =>
         p.startTime < p.startTime.Subtract(p.startTime.TimeOfDay).Add(endOffset) 
          && p.endTime > p.startTime.Subtract(p.startTime.TimeOfDay).Add(startOffset)
          ||
           p.startTime < p.startTime.Subtract(p.startTime.TimeOfDay).Add(endOffset.Add(new TimeSpan(-1, 0, 0, 0)))
           && p.endTime > p.startTime.Subtract(p.startTime.TimeOfDay).Add(startOffset.Add(new TimeSpan(-1, 0, 0, 0)))
         ||
                p.startTime < p.startTime.Subtract(p.startTime.TimeOfDay).Add(endOffset.Add(new TimeSpan(1, 0, 0, 0)))
            && p.endTime > p.startTime.Subtract(p.startTime.TimeOfDay).Add(startOffset.Add(new TimeSpan(1, 0, 0, 0)))
          );
      return query;
    }*/

    public static IList<Program> RetrieveEveryTimeOnEveryChannel(string title)
    {
      IList<Program> retrieveByTitleAndTimesInterval =
        RetrieveByTitleAndTimesInterval(title, DateTime.Now, DateTime.MaxValue);
      return retrieveByTitleAndTimesInterval;
    }

    public static IList<Program> RetrieveByTitleAndTimesInterval(string title, DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var query = context.Programs.IncludeAllRelations()
          .Where(p => p.Title == title && p.EndTime >= startTime && p.StartTime <= endTime);
        return query.ToList();
      }
    }

    public static IList<Program> RetrieveEveryTimeOnThisChannel(string title, int channelId)
    {
      return RetrieveEveryTimeOnThisChannel(title, channelId, -1);
    }

    public static IList<Program> RetrieveEveryTimeOnThisChannel(string title, int channelId, int maxDays)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        DateTime startTime = DateTime.Now;
        DateTime endTime = maxDays > 0 ? startTime.AddDays(maxDays) : DateTime.MaxValue;
        return context.Programs
          .IncludeAllRelations()
          .Where(p => p.Title == title && p.EndTime >= startTime && p.EndTime < endTime && p.ChannelId == channelId)
          .ToList();
      }
    }

    public static IList<Program> RetrieveWeeklyEveryTimeOnThisChannel(DateTime startTime, string title, int channelId)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var query = context.Programs
          .IncludeAllRelations()
          .Where(
            p =>
              p.Title == title && p.ChannelId == channelId && p.StartTime >= DateTime.Now &&
              p.EndTime <= DateTime.MaxValue)
          .AddWeekdayConstraint(startTime.DayOfWeek);
        return query.ToList();
      }
    }

    public static IList<Program> RetrieveWeekends(DateTime startTime, DateTime endTime, int channelId)
    {
      return RetrieveWeekends(startTime, endTime, channelId, -1);
    }

    public static IList<Program> RetrieveWeekends(DateTime startTime, DateTime endTime, int channelId, int maxDays)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        DateTime now = DateTime.Now;
        var query = context.Programs
          .IncludeAllRelations()
          .Where(p => p.EndTime >= now && p.ChannelId == channelId)
          .AddWeekendsConstraint()
          .AddTimeRangeConstraint(startTime, endTime);

        if (maxDays > 0)
        {
          query = query.Where(p => p.StartTime < now.AddDays(maxDays));
        }

        return query.ToList();
      }
    }

    public static IList<Program> RetrieveWeekly(DateTime startTime, DateTime endTime, int channelId)
    {
      return RetrieveWeekly(startTime, endTime, channelId, -1);
    }

    public static IList<Program> RetrieveWeekly(DateTime startTime, DateTime endTime, int channelId, int maxDays)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        DateTime now = DateTime.Now;
        var query = context.Programs
          .IncludeAllRelations()
          .Where(p => p.EndTime >= now && p.ChannelId == channelId)
          .AddWeekdayConstraint(startTime.DayOfWeek)
          .AddTimeRangeConstraint(startTime, endTime);
        if (maxDays > 0)
        {
          query = query.Where(p => p.StartTime < now.AddDays(maxDays));
        }
        return query.ToList();
      }
    }

    public static IList<Program> RetrieveWorkingDays(DateTime startTime, DateTime endTime, int channelId)
    {
      return RetrieveWorkingDays(startTime, endTime, channelId, -1);
    }

    public static IList<Program> RetrieveWorkingDays(DateTime startTime, DateTime endTime, int channelId, int maxDays)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        DateTime now = DateTime.Now;
        var query = context.Programs
          .IncludeAllRelations()
          .Where(p => p.EndTime >= now && p.ChannelId == channelId)
          .AddWorkingDaysConstraint()
          .AddTimeRangeConstraint(startTime, endTime);

        if (maxDays > 0)
        {
          query = query.Where(p => p.StartTime < now.AddDays(maxDays));
        }
        return query.ToList();
      }
    }

    public static Program SaveProgram(Program program)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        SynchronizeDateHelpers(program);
        context.Programs.Add(program);
        context.SaveChanges();
        return program;
      }
    }

    private static void SynchronizeDateHelpers(Program program)
    {
      // TODO workaround/hack for Entity Framework not currently (as of 21-11-2011) supporting DayOfWeek and other similar date functions.
      // once this gets sorted, if ever, this code should be refactored. The db helper fields should be deleted as they have no purpose.

      #region hack

      DateTime startDateTime = program.StartTime;
      DateTime endDateTime = program.EndTime;
      program.StartTimeDayOfWeek = (short)startDateTime.DayOfWeek;
      program.EndTimeDayOfWeek = (short)endDateTime.DayOfWeek;
      program.StartTimeOffset = ProgramExtensions.CreateDateTimeFromTimeSpan(startDateTime.TimeOfDay);
      program.EndTimeOffset = ProgramExtensions.CreateDateTimeFromTimeSpan(endDateTime.Subtract(startDateTime.Date));

      #endregion
    }


    public static IList<Program> GetProgramsByTitleAndTimesInterval(string title, DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var programsByTitleAndTimesInterval = context.Programs
            .Include(p => p.ProgramCategory)
            .Where(p => p.Title == title && p.EndTime >= startTime && p.StartTime <= startTime);
        return programsByTitleAndTimesInterval.ToList();
      }
    }

    public static Program GetProgramsByTitleTimesAndChannel(string programName, DateTime startTime, DateTime endTime, int idChannel)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var programsByTitleTimesAndChannel = context.Programs
          .Include(p => p.ProgramCategory)
          .FirstOrDefault(p => p.Title == programName && p.StartTime == startTime && p.EndTime == endTime && p.ChannelId == idChannel);
        return programsByTitleTimesAndChannel;
      }
    }

    public static IList<Program> GetProgramsByState(ProgramState state)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var programsByState = context.Programs.Include(p => p.ProgramCategory).Where(p => (p.State & (int)state) == (int)state);
        return programsByState.ToList();
      }
    }

    public static Program GetProgramByTitleAndTimes(string programName, DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var programByTitleAndTimes = context.Programs
          .Include(p => p.ProgramCategory)
          .FirstOrDefault(p => p.Title == programName && p.StartTime == startTime && p.EndTime == endTime);
        return programByTitleAndTimes;
      }
    }

    public static IList<Program> GetProgramsByDescription(string searchCriteria, MediaTypeEnum mediaType,
      StringComparisonEnum stringComparison)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        IQueryable<Program> query = context.Programs
          .IncludeAllRelations()
          .GetProgramsByDescription(searchCriteria, stringComparison)
          .Where(p => p.Channel.MediaType == (int)mediaType);
        return query.ToList();
      }
    }

    public static IList<Program> GetProgramsByDescription(string searchCriteria, StringComparisonEnum stringComparison)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        //todo gibman: check if those stringComparison that contains reg. expr. are indeed working.. client calls with reg exp.
        IQueryable<Program> programsByDescription = context.Programs
          .IncludeAllRelations()
          .GetProgramsByDescription(searchCriteria, stringComparison);
        return programsByDescription.ToList();
      }
    }

    public static IList<Program> GetProgramsByTitle(string searchCriteria, StringComparisonEnum stringComparison)
    {
      //todo gibman: check if those stringComparison that contains reg. expr. are indeed working.. client calls with reg exp.
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        IQueryable<Program> programsByTitle = context.Programs
          .IncludeAllRelations()
          .GetProgramsByTitle(searchCriteria, stringComparison);
        return programsByTitle.ToList();
      }
    }

    public IList<Program> GetProgramsByCategory(string searchCriteria, StringComparisonEnum stringComparison)
    {
      //todo gibman: check if those stringComparison that contains reg. expr. are indeed working.. client calls with reg exp.
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        IQueryable<Program> programsByCategory = context.Programs
          .IncludeAllRelations()
          .GetProgramsByCategory(searchCriteria, stringComparison);
        return programsByCategory.ToList();
      }
    }

    public static IList<Program> GetProgramsByTitle(string searchCriteria, MediaTypeEnum mediaType, StringComparisonEnum stringComparisonEnum)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        IQueryable<Program> programsByTitle = context.Programs
          .IncludeAllRelations()
          .GetProgramsByTitle(searchCriteria, stringComparisonEnum)
          .Where(p => p.Channel.MediaType == (int)mediaType);
        return programsByTitle.ToList();
      }
    }

    public static Program GetProgram(int idProgram)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var findOne = context.Programs
          .Include(p => p.ProgramCategory)
          .FirstOrDefault(p => p.ProgramId == idProgram);
        return findOne;
      }
    }

    public static void SetSingleStateSeriesPending(DateTime startTime, int idChannel, string programName)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        Program program = context.Programs.FirstOrDefault(p => p.Title == programName && p.StartTime == startTime && p.ChannelId == idChannel);
        if (program != null)
        {
          // TODO check if changes by ProgramBLL are visible to EFCore change tracker
          var programBll = new ProgramBLL(program) { IsRecordingOncePending = false, IsRecordingSeriesPending = true };
          //programRepository.Update(programBll.Entity);
          context.SaveChanges();
        }
      }
    }

    public static IList<Program> GetProgramsForAllChannels(IEnumerable<Channel> channels)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var buildContainsExpression = channels.Select(channel => channel.ChannelId).BuildContainsExpression<Channel, int>(e => e.ChannelId);
        var context2 = context;
        IQueryable<Program> query = context.Programs.Where(p => context2.Channels.Where(buildContainsExpression).Any(c => c.ChannelId == p.ChannelId));
        return query.ToList();
      }
    }

    public static IDictionary<int, IList<Program>> GetProgramsForAllChannels(DateTime startTime, DateTime endTime, IEnumerable<Channel> channels)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var buildContainsExpression = channels.Select(channel => channel.ChannelId).BuildContainsExpression<Channel, int>(e => e.ChannelId);
        var context2 = context;
        IQueryable<Program> query = context.Programs.Where(
            p => (p.EndTime > startTime && p.EndTime < endTime) ||
                 (p.StartTime >= startTime && p.StartTime <= endTime) ||
                 (p.StartTime <= startTime && p.EndTime >= endTime)
                 && context2.Channels.Where(buildContainsExpression)
                   .Any(c => c.ChannelId == p.ChannelId)).OrderBy(p => p.StartTime).Include(p => p.ProgramCategory)
          .Include(p => p.Channel);

        IDictionary<int, IList<Program>> maps = new Dictionary<int, IList<Program>>();

        foreach (Program program in query)
        {
          if (!maps.ContainsKey(program.ChannelId))
          {
            maps[program.ChannelId] = new List<Program>();
          }

          maps[program.ChannelId].Add(program);
        }

        return maps;
      }
    }

    public static IList<Program> GetProgramsByTitleAndCategoryAndMediaType(string categoryCriteriea,
      string titleCriteria, MediaTypeEnum mediaType, StringComparisonEnum stringComparisonCategory,
      StringComparisonEnum stringComparisonTitle)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        IQueryable<Program> query = context.Programs
          .GetProgramsByTitle(titleCriteria, stringComparisonTitle)
          .GetProgramsByCategory(categoryCriteriea, stringComparisonCategory)
          .Where(p => p.Channel.MediaType == (int)mediaType)
          .IncludeAllRelations();
        return query.ToList();
      }
    }

    public static IList<Program> GetProgramsByTimesInterval(DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var programsByTimesInterval = context.Programs.GetProgramsByTimesInterval(startTime, endTime);
        return programsByTimesInterval.ToList();
      }
    }

    public static void SynchProgramStates(int idSchedule)
    {
      // We need the CanceledSchedules included here to properly update programs state
      SynchProgramStates(new ScheduleBLL(ScheduleManagement.GetSchedule(idSchedule, ScheduleIncludeRelationEnum.CanceledSchedules)));
    }

    public static void SynchProgramStates(ScheduleBLL schedule)
    {
      if (schedule == null || schedule.Entity == null)
      {
        return;
      }

      IEnumerable<Program> programs = GetProgramsForSchedule(schedule.Entity);

      foreach (var prog in programs)
      {
        var programBll = new ProgramBLL(prog);
        // If a single "record once" schedule was deleted, reset the pending state.
        // TODO Morpheus_xx, 2020-02-01: check for EF core version of change tracking
        if (/*schedule.Entity.ChangeTracker.State == ObjectState.Deleted ||*/ schedule.IsSerieIsCanceled(schedule.GetSchedStartTimeForProg(prog)))
        {
          // program has been cancelled so reset any pending recording flags
          ResetPendingState(programBll);
        }
        else
        {
          bool isPartialRecording = schedule.IsPartialRecording(prog);
          if (schedule.Entity.ScheduleType == (int)ScheduleRecordingType.Once)
          {
            // is one off recording that is still active so set pending flags accordingly
            programBll.IsRecordingOncePending = true;
            programBll.IsRecordingSeriesPending = false;
            programBll.IsPartialRecordingSeriesPending = false;
            SaveProgram(programBll.Entity);
          }
          else if (isPartialRecording)
          {
            // is part of a series recording but is a time based schedule and program times do not
            // match up with schedule times so flag as partial recording
            programBll.IsRecordingOncePending = false;
            programBll.IsRecordingSeriesPending = false;
            programBll.IsPartialRecordingSeriesPending = true;
            SaveProgram(programBll.Entity);
          }
          else
          {
            // is part of a series recording but is not a partial recording
            programBll.IsRecordingOncePending = false;
            programBll.IsRecordingSeriesPending = true;
            programBll.IsPartialRecordingSeriesPending = false;
            SaveProgram(programBll.Entity);
          }
        }
      }
    }

    private static void ResetPendingState(ProgramBLL prog)
    {
      if (prog != null)
      {
        prog.IsRecordingOncePending = false;
        prog.IsRecordingSeriesPending = false;
        prog.IsPartialRecordingSeriesPending = false;

        SaveProgram(prog.Entity);
      }
    }

    public static IList<Program> GetProgramsForSchedule(Schedule schedule)
    {
      IList<Program> progsEntities = new List<Program>();
      switch (schedule.ScheduleType)
      {
        case (int)ScheduleRecordingType.Once:
          var prgOnce = RetrieveByTitleTimesAndChannel(schedule.ProgramName, schedule.StartTime, schedule.EndTime,
            schedule.ChannelId);
          if (prgOnce != null)
          {
            progsEntities.Add(prgOnce);
          }

          return progsEntities;

        case (int)ScheduleRecordingType.Daily:
          progsEntities = RetrieveDaily(schedule.StartTime, schedule.EndTime, schedule.ChannelId);
          break;

        case (int)ScheduleRecordingType.EveryTimeOnEveryChannel:
          progsEntities = RetrieveEveryTimeOnEveryChannel(schedule.ProgramName).ToList();
          return progsEntities;

        case (int)ScheduleRecordingType.EveryTimeOnThisChannel:
          progsEntities = RetrieveEveryTimeOnThisChannel(schedule.ProgramName, schedule.ChannelId).ToList();
          return progsEntities;

        case (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel:
          progsEntities =
            RetrieveWeeklyEveryTimeOnThisChannel(schedule.StartTime, schedule.ProgramName, schedule.ChannelId).ToList();
          return progsEntities;

        case (int)ScheduleRecordingType.Weekends:
          progsEntities = RetrieveWeekends(schedule.StartTime, schedule.EndTime, schedule.ChannelId);
          break;

        case (int)ScheduleRecordingType.Weekly:
          progsEntities = RetrieveWeekly(schedule.StartTime, schedule.EndTime, schedule.ChannelId);
          break;

        case (int)ScheduleRecordingType.WorkingDays:
          progsEntities = RetrieveWorkingDays(schedule.StartTime, schedule.EndTime, schedule.ChannelId);
          break;
      }

      return progsEntities;
    }

    /*public static IList<Program> ConvertGentleToEntities(IEnumerable<Gentle.Program> progs)
    {
      IList<Program> prgEntities = new List<Program>();
      foreach (var program in progs)
      {
        Program entity = new Program();
        entity.classification = program.Classification;
        entity.description = program.Description;
        entity.endTime = program.endTime;
        entity.startTime = program.startTime;
        entity.title = program.Title;
        entity.starRating = program.StarRating;
        entity.idChannel = program.idChannel;
        entity.idProgram = program.ProgramId;
        entity.episodeName = program.EpisodeName;
        entity.episodeNum = program.EpisodeNum;
        entity.episodePart = program.EpisodePart;
        entity.originalAirDate = program.OriginalAirDate;
        entity.parentalRating = program.ParentalRating;
        prgEntities.Add(entity);
      }
      return prgEntities;
    }*/

    public static void ResetAllStates()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        foreach (ProgramBLL program in context.Programs.Where(p => p.State != 0).Select(p => new ProgramBLL(p)))
        {
          ResetPendingState(program);
        }
      }

      //TODO implement Future recording as discussed
    }

    public static IList<Program> RetrieveCurrentRunningByTitle(string programName, int preRecordInterval, int postRecordInterval)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        DateTime now = DateTime.Now;
        DateTime withPreRecord = now.AddMinutes(preRecordInterval);
        return context.Programs.Where(p => p.Title == programName && p.StartTime <= withPreRecord && p.EndTime > now).ToList();
      }
    }

    public static ProgramCategory GetProgramCategoryByName(string category)
    {
      ProgramCategory programCategory = EntityCacheHelper.Instance.ProgramCategoryCache.GetOrUpdateFromCache(category,
        delegate
        {
          using (TvEngineDbContext context = new TvEngineDbContext())
          {
            return context.ProgramCategories.FirstOrDefault(p => p.Category == category);
          }
        });
      return programCategory;
    }

    public static IList<string> ListAllDistinctCreditRoles()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ProgramCredits.Select(p => p.Role).Distinct().ToList();
      }
    }

    public static IList<ProgramCategory> ListAllCategories()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ProgramCategories.ToList();
      }
    }

    public static IList<ProgramCredit> ListAllCredits()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ProgramCredits.ToList();
      }
    }

    public static IList<Program> RetrieveByChannelAndTimesInterval(int channelId, DateTime startTime, DateTime endTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var query = context.Programs
          .IncludeAllRelations()
          .Where(p => p.ChannelId == channelId && p.StartTime >= startTime && p.EndTime <= endTime);
        return query.ToList();
      }
    }

    public static void DeleteProgram(int idProgram)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var program = context.Programs.FirstOrDefault(p => p.ProgramCategoryId == idProgram);
        if (program != null)
        {
          context.Programs.Remove(program);
          context.SaveChanges();
        }
      }
    }

    public static void DeleteOldPrograms()
    {
      DateTime dtYesterday = DateTime.Now.AddHours(-SettingsManagement.EpgKeepDuration);
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var programs = context.Programs.Where(p => p.EndTime < dtYesterday);
        if (programs.Any())
        {
          context.Programs.RemoveRange(programs);
          context.SaveChanges();
        }
      }
    }

    public static void DeleteOldPrograms(int idChannel)
    {
      DateTime dtYesterday = DateTime.Now.AddHours(-SettingsManagement.EpgKeepDuration);
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var programs = context.Programs.Where(p => p.EndTime < dtYesterday && p.ChannelId == idChannel);
        if (programs.Any())
        {
          context.Programs.RemoveRange(programs);
          context.SaveChanges();
        }
      }
    }

    public static IList<Program> GetPrograms(int idChannel, DateTime startTime)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Programs
          .Where(p => p.ChannelId == idChannel && p.StartTime >= startTime)
          .OrderBy(p => p.StartTime)
          .ToList();
      }
    }

    public static DateTime GetNewestProgramForChannel(int idChannel)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        Program program = context.Programs.Where(p => p.ChannelId == idChannel)
          .OrderByDescending(p => p.StartTime).FirstOrDefault();
        if (program != null)
        {
          return program.StartTime;
        }
        else
        {
          return DateTime.MinValue;
        }
      }
    }

    public static IList<Program> GetProgramExists(int idChannel, DateTime startTime, DateTime endTime)
    {
      /*
         string sub1 =
        String.Format("( (StartTime >= '{0}' and StartTime < '{1}') or ( EndTime > '{0}' and EndTime <= '{1}' ) )",
                      startTime.ToString(GetDateTimeString(), mmddFormat),
                      endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub2 = String.Format("(StartTime < '{0}' and EndTime > '{1}')",
                                  startTime.ToString(GetDateTimeString(), mmddFormat),
                                  endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.ChannelId);
      sb.AddConstraint(string.Format("({0} or {1}) ", sub1, sub2));
      sb.AddOrderByField(true, "starttime");
      */
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var query = context.Programs.GetProgramsByTimesInterval(startTime, endTime);
        query = query.Where(p => p.ChannelId == idChannel);
        return query.ToList();
      }
    }

    public static IList<Program> SavePrograms(IEnumerable<Program> programs)
    {
      IList<Program> progs = programs.ToList();
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        SynchronizeDateHelpers(progs);
        context.Programs.AddRange(programs);
        context.SaveChanges();
        //programRepository.AttachEntityIfChangeTrackingDisabled(programRepository.ObjectContext.Programs, progs);
        //programRepository.ApplyChanges(programRepository.ObjectContext.Programs, progs);
        //programRepository.UnitOfWork.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
      }

      return progs;
    }

    private static void SynchronizeDateHelpers(IEnumerable<Program> programs)
    {
      foreach (Program program in programs)
      {
        SynchronizeDateHelpers(program);
      }
    }

    public static IQueryable<Program> GetNowProgramsForChannelGroup(TvEngineDbContext context, int idGroup)
    {
      IQueryable<int> channels = context.Channels.Where(c => c.GroupMaps.Any(g => g.ChannelGroupId == idGroup))
        .Select(g => g.ChannelId);

      DateTime now = DateTime.Now;
      IQueryable<Program> programs =
        context.Programs.Where(p => channels.Contains(p.ChannelId) && p.EndTime > now && p.StartTime <= now);
      return programs;
    }

    public static IQueryable<Program> GetNextProgramsForChannelGroup(TvEngineDbContext context, int idGroup)
    {
      IQueryable<int> channels = context.Channels.Where(c => c.GroupMaps.Any(g => g.ChannelGroupId == idGroup))
        .Select(g => g.ChannelId);
      DateTime now = DateTime.Now;

      var q = context.Programs.Where(p =>
          channels.Contains(p.ChannelId) && p.StartTime > now)
        .GroupBy(p => p.ChannelId)
        .Select(pg => new
        {
          idChannel = pg.Key,
          minStartTime = pg.Min(p => p.StartTime)
        });
      IQueryable<Program> programs = context.Programs.Where(p =>
        q.Any(pmin => p.ChannelId == pmin.idChannel && p.StartTime == pmin.minStartTime));

      return programs;
    }

    //public static void DeleteAllProgramsWithChannelId(int idChannel)
    //{
    //  Delete<Program>(p => p.IdChannel == idChannel);
    //  UnitOfWork.SaveChanges();
    //}

    //public Program GetProgramAt(DateTime date, string title)
    //{
    //  var programAt = GetQuery<Program>().Where(p => p.Title == title && p.EndTime > date && p.StartTime <= date);
    //  programAt = IncludeAllRelations(programAt).OrderBy(p => p.StartTime);
    //  return programAt.FirstOrDefault();
    //}
  }

  public static class ProgramExtensions
  {
    public static IQueryable<Program> AddWeekendsConstraint(this IQueryable<Program> query)
    {
      DayOfWeek firstWeekendDay = WeekEndTool.FirstWeekendDay;
      DayOfWeek secondWeekendDay = WeekEndTool.SecondWeekendDay;

      // TODO workaround/hack for Entity Framework not currently (as of 21-11-2011) supporting DayOfWeek and other similar date functions.
      // once this gets sorted, if ever, this code should be refactored. The db helper fields should be deleted as they have no purpose.
      #region hack
      query = query.Where(p => p.StartTimeDayOfWeek == (int)firstWeekendDay || p.StartTimeDayOfWeek == (int)secondWeekendDay);
      #endregion

      //query = query.Where(p => p.startTime.DayOfWeek == firstWeekendDay || p.startTime.DayOfWeek == secondWeekendDay);

      return query;
    }

    // TODO workaround/hack for Entity Framework not currently (as of 21-11-2011) supporting DayOfWeek and other similar date functions.
    // once this gets sorted, if ever, this code should be refactored. The db helper fields should be deleted as they have no purpose.
    public static IQueryable<Program> AddTimeRangeConstraint(this IQueryable<Program> query, DateTime startTime, DateTime endTime)
    {
      TimeSpan startOffset = startTime.TimeOfDay;
      TimeSpan endOffset = endTime - startTime.Date;
      if (startOffset > endOffset)
      {
        endOffset = endOffset.Add(new TimeSpan(1, 0, 0, 0));
      }

      DateTime endDateTimeOffset1 = CreateDateTimeFromTimeSpan(endOffset.Add(new TimeSpan(-1, 0, 0, 0)));
      DateTime endDateTimeOffset2 = CreateDateTimeFromTimeSpan(endOffset.Add(new TimeSpan(1, 0, 0, 0)));
      DateTime endDateTimeOffset = CreateDateTimeFromTimeSpan(endOffset);
      DateTime startDateTimeOffset1 = CreateDateTimeFromTimeSpan(startOffset.Add(new TimeSpan(-1, 0, 0, 0)));
      DateTime startDateTimeOffset2 = CreateDateTimeFromTimeSpan(startOffset.Add(new TimeSpan(1, 0, 0, 0)));
      DateTime startDateTimeOffset = CreateDateTimeFromTimeSpan(startOffset);

      query = query.Where(p =>
        p.StartTimeOffset < endDateTimeOffset
        && p.EndTimeOffset > startDateTimeOffset
        ||
        p.StartTimeOffset < endDateTimeOffset1
        && p.EndTimeOffset > startDateTimeOffset1
        ||
        p.StartTimeOffset < endDateTimeOffset2
        && p.EndTimeOffset > startDateTimeOffset2
      );
      return query;
    }

    public static IQueryable<Program> AddWorkingDaysConstraint(this IQueryable<Program> query)
    {
      var firstWeekendDay = WeekEndTool.FirstWeekendDay;
      var secondWeekendDay = WeekEndTool.SecondWeekendDay;

      // TODO workaround/hack for Entity Framework not currently (as of 21-11-2011) supporting DayOfWeek and other similar date functions.
      // once this gets sorted, if ever, this code should be refactored. The db helper fields should be deleted as they have no purpose.

      #region hack

      query = query.Where(p =>
        p.StartTimeDayOfWeek != (int)firstWeekendDay && p.StartTimeDayOfWeek != (int)secondWeekendDay);

      #endregion

      //query = query.Where(p => p.startTime.DayOfWeek != firstWeekendDay && p.startTime.DayOfWeek != secondWeekendDay);
      return query;
    }

    public static DateTime CreateDateTimeFromTimeSpan(TimeSpan timeSpan)
    {
      var date = new DateTime(2000, 1, 1).Add(timeSpan);
      return date;
    }

  }
}
