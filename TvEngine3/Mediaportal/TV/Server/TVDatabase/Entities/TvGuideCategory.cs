using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(ProgramCategory))]
  public class TvGuideCategory
  {
    [DataMember]
    public int TvGuideCategoryId { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public bool IsMovie { get; set; }
    [DataMember]
    public bool IsEnabled { get; set; }

    [DataMember]
    public virtual List<ProgramCategory> ProgramCategories { get; set; } = new List<ProgramCategory>();
  }
}
