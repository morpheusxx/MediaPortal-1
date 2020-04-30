using System;
using AutoMapper;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class ScheduleFactory
  {
    private static readonly Mapper _mapper;
    static ScheduleFactory()
    {
      MapperConfiguration config = new MapperConfiguration(c => { c.CreateMap<Schedule, Schedule>(); });
      _mapper = new Mapper(config);
    }

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

    public static Schedule Clone(Schedule baseSchedule)
    {
      var clone = _mapper.Map<Schedule>(baseSchedule);
      // Clear primary key, so EF core creates a new key when saving. Otherwise you get unique constraint violations.
      clone.ScheduleId = 0;
      return clone;
    }
  }
}
