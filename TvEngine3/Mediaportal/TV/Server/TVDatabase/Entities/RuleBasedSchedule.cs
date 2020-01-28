using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class RuleBasedSchedule
  {
    public int RuleBasedScheduleId { get; set; }
    public string ScheduleName { get; set; }
    public int MaxAirings { get; set; }
    public int Priority { get; set; }
    public string Directory { get; set; }
    public int Quality { get; set; }
    public int KeepMethod { get; set; }
    public DateTime? KeepDate { get; set; }
    public int PreRecordInterval { get; set; }
    public int PostRecordInterval { get; set; }
    public string Rules { get; set; }
  }
}
