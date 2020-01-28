using System;
using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Recording
  {
    public int RecordingId { get; set; }
    public int? ChannelId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
    public int KeepUntil { get; set; }
    public DateTime? KeepUntilDate { get; set; }
    public int TimesWatched { get; set; }
    public int StopTime { get; set; }
    public string EpisodeName { get; set; }
    public string SeriesNum { get; set; }
    public string EpisodeNum { get; set; }
    public string EpisodePart { get; set; }
    public bool IsRecording { get; set; }
    public int? ScheduleId { get; set; }
    public int MediaType { get; set; }
    public int? ProgramCategoryId { get; set; }

    public Channel Channel { get; set; }
    public Schedule Schedule { get; set; }
    public virtual ICollection<RecordingCredit> RecordingCredits { get; set; }
    public ProgramCategory ProgramCategory { get; set; }
  }
}
