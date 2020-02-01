using System;
using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Program
  {
    public int ProgramId { get; set; }
    public int ChannelId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string SeriesNum { get; set; }
    public string EpisodeNum { get; set; }
    public DateTime? OriginalAirDate { get; set; }
    public string Classification { get; set; }
    public int StarRating { get; set; }
    public int ParentalRating { get; set; }
    public string EpisodeName { get; set; }
    public string EpisodePart { get; set; }
    public int State { get; set; }
    public bool PreviouslyShown { get; set; }
    public int? ProgramCategoryId { get; set; }
    public short StartTimeDayOfWeek { get; set; }
    public short EndTimeDayOfWeek { get; set; }
    public DateTime EndTimeOffset { get; set; }
    public DateTime StartTimeOffset { get; set; }

    public Channel Channel { get; set; }
    public ProgramCategory ProgramCategory { get; set; }
    public virtual ICollection<ProgramCredit> ProgramCredits { get; set; }
    public virtual ICollection<PersonalTVGuideMap> PersonalTVGuideMaps { get; set; }
  }
}
