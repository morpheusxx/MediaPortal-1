using System;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class ScheduleFactory
  {
    public static DateTime MinSchedule = new DateTime(2000, 1, 1);
    public static readonly int HighestPriority = Int32.MaxValue;

    public static Schedule CreateSchedule(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      var schedule = new Schedule
      {
        ChannelId = idChannel,
        ParentScheduleId = null,
        ProgramName = programName,
        Canceled = MinSchedule,
        Directory = "",
        EndTime = endTime,
        KeepDate = MinSchedule,
        KeepMethod = (int)KeepMethodType.UntilSpaceNeeded,
        MaxAirings = Int32.MaxValue,
        PostRecordInterval = 0,
        PreRecordInterval = 0,
        Priority = 0,
        Quality = 0,
        Series = false,
        StartTime = startTime
      };
      return schedule;
    }
  }
}
