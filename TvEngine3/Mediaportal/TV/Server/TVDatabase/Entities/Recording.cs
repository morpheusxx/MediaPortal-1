using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Channel))]
  [KnownType(typeof(Schedule))]
  [KnownType(typeof(RecordingCredit))]
  [KnownType(typeof(ProgramCategory))]
  public class Recording
  {
    [DataMember]
    public int RecordingId { get; set; }
    [DataMember]
    public int? ChannelId { get; set; }
    [DataMember]
    public DateTime StartTime { get; set; }
    [DataMember]
    public DateTime EndTime { get; set; }
    [DataMember]
    public string Title { get; set; }
    [DataMember]
    public string Description { get; set; }
    [DataMember]
    public string FileName { get; set; }
    [DataMember]
    public int KeepUntil { get; set; }
    [DataMember]
    public DateTime? KeepUntilDate { get; set; }
    [DataMember]
    public int TimesWatched { get; set; }
    [DataMember]
    public int StopTime { get; set; }
    [DataMember]
    public string EpisodeName { get; set; }
    [DataMember]
    public string SeriesNum { get; set; }
    [DataMember]
    public string EpisodeNum { get; set; }
    [DataMember]
    public string EpisodePart { get; set; }
    [DataMember]
    public bool IsRecording { get; set; }
    [DataMember]
    public int? ScheduleId { get; set; }
    [DataMember]
    public int MediaType { get; set; }
    [DataMember]
    public int? ProgramCategoryId { get; set; }

    [DataMember]
    public Channel Channel { get; set; }
    [DataMember]
    public Schedule Schedule { get; set; }
    [DataMember]
    public virtual ICollection<RecordingCredit> RecordingCredits { get; set; }
    [DataMember]
    public ProgramCategory ProgramCategory { get; set; }
  }
}
