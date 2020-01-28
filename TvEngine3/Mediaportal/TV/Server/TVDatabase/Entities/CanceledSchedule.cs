using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class CanceledSchedule
  {
    public int CanceledScheduleId { get; set; }
    public int ScheduleId { get; set; }
    public int ChannelId { get; set; }
    public DateTime CancelDateTime { get; set; }
    public Schedule Schedule { get; set; }
  }
}
