using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class ProgramCategory
  {
    public int ProgramCategoryId { get; set; }
    public string Category { get; set; }
    public int? TvGuideCategoryId { get; set; }

    public virtual ICollection<Program> Programs { get; set; }
    public virtual ICollection<Recording> Recordings { get; set; }
    public virtual ICollection<History> Histories { get; set; }
    public TvGuideCategory TvGuideCategory { get; set; }

    public override string ToString()
    {
      return Category;
    }
  }
}
