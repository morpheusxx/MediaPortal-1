using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Channel))]
  [KnownType(typeof(Schedule))]
  [KnownType(typeof(Recording))]
  [KnownType(typeof(CanceledSchedule))]
  [KnownType(typeof(Conflict))]
  public class Schedule
  {
    public static DateTime MinSchedule = new DateTime(2000, 1, 1);
    public static readonly int HighestPriority = Int32.MaxValue;

    [DataMember]
    public int ScheduleId { get; set; }
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public int ScheduleType { get; set; }
    [DataMember]
    public string ProgramName { get; set; }
    [DataMember]
    public DateTime StartTime { get; set; }
    [DataMember]
    public DateTime EndTime { get; set; }
    [DataMember]
    public int MaxAirings { get; set; }
    [DataMember]
    public int Priority { get; set; }
    [DataMember]
    public string Directory { get; set; }
    [DataMember]
    public int Quality { get; set; }
    [DataMember]
    public int KeepMethod { get; set; }
    [DataMember]
    public DateTime? KeepDate { get; set; }
    [DataMember]
    public int PreRecordInterval { get; set; }
    [DataMember]
    public int PostRecordInterval { get; set; }
    [DataMember]
    public DateTime Canceled { get; set; }
    [DataMember]
    public bool Series { get; set; }
    [DataMember]
    public int? ParentScheduleId { get; set; }

    [DataMember]
    public Channel Channel { get; set; }
    [DataMember]
    public virtual ICollection<Schedule> SubSchedules { get; set; }
    [DataMember]
    public Schedule ParentSchedule { get; set; }
    [DataMember]
    public virtual ICollection<Recording> Recordings { get; set; }
    [DataMember]
    public virtual ICollection<CanceledSchedule> CanceledSchedules { get; set; }
    [DataMember]
    public virtual ICollection<Conflict> Conflicts { get; set; }
    //public virtual ICollection<Conflict> ConflictingSchedules { get; set; }
  }
}
