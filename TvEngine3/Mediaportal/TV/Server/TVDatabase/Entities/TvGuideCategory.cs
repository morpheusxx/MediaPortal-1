using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class TvGuideCategory
  {
    public int TvGuideCategoryId { get; set; }
    public string Name { get; set; }
    public bool IsMovie { get; set; }
    public bool IsEnabled { get; set; }

    public virtual ICollection<ProgramCategory> ProgramCategories { get; set; }
  }
}
