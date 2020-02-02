using System;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Card))]
  [KnownType(typeof(Channel))]
  [KnownType(typeof(Schedule))]
  public class Conflict
  {
    [DataMember]
    public int ConflictId { get; set; }
    [DataMember]
    public int ScheduleId { get; set; }
    [DataMember]
    public int ConflictingScheduleId { get; set; }
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public DateTime ConflictDate { get; set; }
    [DataMember]
    public int? CardId { get; set; }

    [DataMember]
    public Card Card { get; set; }
    [DataMember]
    public Channel Channel { get; set; }
    [DataMember]
    public Schedule Schedule { get; set; }
    [DataMember]
    public Schedule ConflictingSchedule { get; set; }
  }
}
