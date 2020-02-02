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
    public virtual ICollection<GroupMap> GroupMaps { get; set; }
    [DataMember]
    public virtual ICollection<Recording> Recordings { get; set; }
    [DataMember]
    public virtual ICollection<Program> Programs { get; set; }
    [DataMember]
    public virtual ICollection<ChannelMap> ChannelMaps { get; set; }
    [DataMember]
    public virtual ICollection<Schedule> Schedules { get; set; }
    [DataMember]
    public virtual ICollection<History> Histories { get; set; }
    [DataMember]
    public virtual ICollection<TuningDetail> TuningDetails { get; set; }
    [DataMember]
    public virtual ICollection<TvMovieMapping> TvMovieMappings { get; set; }

    [DataMember]
    public virtual ICollection<ChannelLinkageMap> ChannelLinkMaps { get; set; }
    [DataMember]
    public virtual ICollection<ChannelLinkageMap> ChannelPortalMaps { get; set; }
    [DataMember]
    public virtual ICollection<Conflict> Conflicts { get; set; }

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
