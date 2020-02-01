using System;
using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Schedule
  {
    public static DateTime MinSchedule = new DateTime(2000, 1, 1);
    public static readonly int HighestPriority = Int32.MaxValue;

    public int ScheduleId { get; set; }
    public int ChannelId { get; set; }
    public int ScheduleType { get; set; }
    public string ProgramName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxAirings { get; set; }
    public int Priority { get; set; }
    public string Directory { get; set; }
    public int Quality { get; set; }
    public int KeepMethod { get; set; }
    public DateTime? KeepDate { get; set; }
    public int PreRecordInterval { get; set; }
    public int PostRecordInterval { get; set; }
    public DateTime Canceled { get; set; }
    public bool Series { get; set; }
    public int? ParentScheduleId { get; set; }

    public Channel Channel { get; set; }
    public virtual ICollection<Schedule> Schedules { get; set; }
    public Schedule ParentSchedule { get; set; }
    public virtual ICollection<Recording> Recordings { get; set; }
    public virtual ICollection<CanceledSchedule> CanceledSchedules { get; set; }
    public virtual ICollection<Conflict> Conflicts { get; set; }
    //public virtual ICollection<Conflict> ConflictingSchedules { get; set; }
  }
}
