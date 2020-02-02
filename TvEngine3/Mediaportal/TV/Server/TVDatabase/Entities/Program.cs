using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Channel))]
  [KnownType(typeof(ProgramCategory))]
  [KnownType(typeof(ProgramCredit))]
  [KnownType(typeof(PersonalTVGuideMap))]
  public partial class Program
  {
    [DataMember]
    public int ProgramId { get; set; }
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public DateTime StartTime { get; set; }
    [DataMember]
    public DateTime EndTime { get; set; }
    [DataMember]
    public string Title { get; set; }
    [DataMember]
    public string Description { get; set; }
    [DataMember]
    public string SeriesNum { get; set; }
    [DataMember]
    public string EpisodeNum { get; set; }
    [DataMember]
    public DateTime? OriginalAirDate { get; set; }
    [DataMember]
    public string Classification { get; set; }
    [DataMember]
    public int StarRating { get; set; }
    [DataMember]
    public int ParentalRating { get; set; }
    [DataMember]
    public string EpisodeName { get; set; }
    [DataMember]
    public string EpisodePart { get; set; }
    [DataMember]
    public int State { get; set; }
    [DataMember]
    public bool PreviouslyShown { get; set; }
    [DataMember]
    public int? ProgramCategoryId { get; set; }
    [DataMember]
    public short StartTimeDayOfWeek { get; set; }
    [DataMember]
    public short EndTimeDayOfWeek { get; set; }
    [DataMember]
    public DateTime EndTimeOffset { get; set; }
    [DataMember]
    public DateTime StartTimeOffset { get; set; }

    [DataMember]
    public Channel Channel { get; set; }
    [DataMember]
    public ProgramCategory ProgramCategory { get; set; }
    [DataMember]
    public virtual ICollection<ProgramCredit> ProgramCredits { get; set; }
    [DataMember]
    public virtual ICollection<PersonalTVGuideMap> PersonalTVGuideMaps { get; set; }
  }
}
