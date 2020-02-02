using System;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  public class Timespan
  {
    [DataMember]
    public int TimespanId { get; set; }
    [DataMember]
    public DateTime StartTime { get; set; }
    [DataMember]
    public DateTime EndTime { get; set; }
    [DataMember]
    public int DayOfWeek { get; set; }
    [DataMember]
    public int KeywordId { get; set; }
  }
}
