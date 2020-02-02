using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(PersonalTVGuideMap))]
  [KnownType(typeof(KeywordMap))]
  public class Keyword
  {
    [DataMember]
    public int KeywordId { get; set; }
    [DataMember]
    public string KeywordName { get; set; }
    [DataMember]
    public int Rating { get; set; }
    [DataMember]
    public bool AutoRecord { get; set; }
    [DataMember]
    public int SearchIn { get; set; }

    [DataMember]
    public virtual ICollection<PersonalTVGuideMap> PersonalTVGuideMaps { get; set; }
    [DataMember]
    public virtual ICollection<KeywordMap> KeywordMaps { get; set; }
  }
}
