using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Card))]
  [KnownType(typeof(CardGroup))]
  public class CardGroupMap
  {
    [DataMember]
    public int CardGroupMapId { get; set; }
    [DataMember]
    public int CardId { get; set; }
    [DataMember]
    public int CardGroupId { get; set; }

    [DataMember]
    public Card Card { get; set; }
    [DataMember]
    public CardGroup CardGroup { get; set; }
  }
}
