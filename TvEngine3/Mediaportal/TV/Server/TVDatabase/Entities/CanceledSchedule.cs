using System;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Schedule))]
  public class CanceledSchedule
  {
    [DataMember]
    public int CanceledScheduleId { get; set; }
    [DataMember]
    public int ScheduleId { get; set; }
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public DateTime CancelDateTime { get; set; }
    [DataMember]
    public Schedule Schedule { get; set; }
  }
}
