using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Channel
  {
    public int ChannelId { get; set; }
    public int TimesWatched { get; set; }
    public DateTime? TotalTimeWatched { get; set; }
    public bool GrabEpg { get; set; }
    public DateTime? LastGrabTime { get; set; }
    public int SortOrder { get; set; }
    public bool VisibleInGuide { get; set; }
    public string ExternalId { get; set; }
    public string DisplayName { get; set; }
    public bool EpgHasGaps { get; set; }
    public int MediaType { get; set; }
    public int ChannelNumber { get; set; }

    public virtual ICollection<GroupMap> GroupMaps { get; set; }
    public virtual ICollection<Recording> Recordings { get; set; }
    public virtual ICollection<Program> Programs { get; set; }
    public virtual ICollection<ChannelMap> ChannelMaps { get; set; }
    public virtual ICollection<Schedule> Schedules { get; set; }
    public virtual ICollection<History> Histories { get; set; }
    public virtual ICollection<TuningDetail> TuningDetails { get; set; }
    public virtual ICollection<TvMovieMapping> TvMovieMappings { get; set; }

    public virtual ICollection<ChannelLinkageMap> ChannelLinkMaps { get; set; }
    public virtual ICollection<ChannelLinkageMap> ChannelPortalMaps { get; set; }
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
