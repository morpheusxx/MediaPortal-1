using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Timespan
  {
    public int TimespanId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DayOfWeek { get; set; }
    public int KeywordId { get; set; }
  }
}
