using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Conflict
  {
    public int ConflictId { get; set; }
    public int ScheduleId { get; set; }
    public int ConflictingScheduleId { get; set; }
    public int ChannelId { get; set; }
    public DateTime ConflictDate { get; set; }
    public int? CardId { get; set; }

    public Card Card { get; set; }
    public Channel Channel { get; set; }
    public Schedule Schedule { get; set; }
    public Schedule ConflictingSchedule { get; set; }
  }
}
