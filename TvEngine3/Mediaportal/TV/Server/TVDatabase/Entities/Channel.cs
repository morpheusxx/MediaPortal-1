using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(GroupMap))]
  [KnownType(typeof(Recording))]
  [KnownType(typeof(Program))]
  [KnownType(typeof(ChannelMap))]
  [KnownType(typeof(Schedule))]
  [KnownType(typeof(History))]
  [KnownType(typeof(TuningDetail))]
  [KnownType(typeof(TvMovieMapping))]
  [KnownType(typeof(ChannelLinkageMap))]
  [KnownType(typeof(Conflict))]
  public class Channel
  {
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public int TimesWatched { get; set; }
    [DataMember]
    public DateTime? TotalTimeWatched { get; set; }
    [DataMember]
    public bool GrabEpg { get; set; }
    [DataMember]
    public DateTime? LastGrabTime { get; set; }
    [DataMember]
    public int SortOrder { get; set; }
    [DataMember]
    public bool VisibleInGuide { get; set; }
    [DataMember]
    public string ExternalId { get; set; }
    [DataMember]
    public string DisplayName { get; set; }
    [DataMember]
    public bool EpgHasGaps { get; set; }
    [DataMember]
    public int MediaType { get; set; }
    [DataMember]
    public int ChannelNumber { get; set; }

    [DataMember]
    public virtual List<GroupMap> GroupMaps { get; set; } = new List<GroupMap>();
    [DataMember]
    public virtual List<Recording> Recordings { get; set; } = new List<Recording>();
    [DataMember]
    public virtual List<Program> Programs { get; set; } = new List<Program>();
    [DataMember]
    public virtual List<ChannelMap> ChannelMaps { get; set; } = new List<ChannelMap>();
    [DataMember]
    public virtual List<Schedule> Schedules { get; set; } = new List<Schedule>();
    [DataMember]
    public virtual List<History> Histories { get; set; } = new List<History>();
    [DataMember]
    public virtual List<TuningDetail> TuningDetails { get; set; } = new List<TuningDetail>();
    [DataMember]
    public virtual List<TvMovieMapping> TvMovieMappings { get; set; } = new List<TvMovieMapping>();
    [DataMember]
    public virtual List<ChannelLinkageMap> ChannelLinkMaps { get; set; } = new List<ChannelLinkageMap>();
    [DataMember]
    public virtual List<ChannelLinkageMap> ChannelPortalMaps { get; set; } = new List<ChannelLinkageMap>();
    [DataMember]
    public virtual List<Conflict> Conflicts { get; set; } = new List<Conflict>();

    public override string ToString()
    {
      return DisplayName;
    }

    public bool IsWebstream()
    {
      return TuningDetails?.Any(detail => detail.ChannelType == 5) ?? false;
    }
  }
}
