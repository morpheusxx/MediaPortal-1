using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Program))]
  [KnownType(typeof(Recording))]
  [KnownType(typeof(History))]
  [KnownType(typeof(TvGuideCategory))]
  public class ProgramCategory
  {
    [DataMember]
    public int ProgramCategoryId { get; set; }
    [DataMember]
    public string Category { get; set; }
    [DataMember]
    public int? TvGuideCategoryId { get; set; }

    [DataMember]
    public virtual List<Program> Programs { get; set; } = new List<Program>();
    [DataMember]
    public virtual List<Recording> Recordings { get; set; } = new List<Recording>();
    [DataMember]
    public virtual List<History> Histories { get; set; } = new List<History>();
    [DataMember]
    public TvGuideCategory TvGuideCategory { get; set; }

    public override string ToString()
    {
      return Category;
    }
  }
}
