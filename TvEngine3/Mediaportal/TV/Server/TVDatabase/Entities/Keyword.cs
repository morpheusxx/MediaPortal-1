using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Keyword
  {
    public int KeywordId { get; set; }
    public string KeywordName { get; set; }
    public int Rating { get; set; }
    public bool AutoRecord { get; set; }
    public int SearchIn { get; set; }

    public virtual ICollection<PersonalTVGuideMap> PersonalTVGuideMaps { get; set; }
    public virtual ICollection<KeywordMap> KeywordMaps { get; set; }
  }
}
