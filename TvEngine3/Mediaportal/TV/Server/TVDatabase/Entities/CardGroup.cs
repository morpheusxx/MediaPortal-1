using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(CardGroupMap))]
  public class CardGroup
  {
    [DataMember]
    public int CardGroupId { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public virtual List<CardGroupMap> CardGroupMaps { get; set; } = new List<CardGroupMap>();
  }
}
