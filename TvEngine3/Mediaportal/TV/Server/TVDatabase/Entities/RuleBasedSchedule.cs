using System;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  public class RuleBasedSchedule
  {
    [DataMember]
    public int RuleBasedScheduleId { get; set; }
    [DataMember]
    public string ScheduleName { get; set; }
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
    public string Rules { get; set; }
  }
}
