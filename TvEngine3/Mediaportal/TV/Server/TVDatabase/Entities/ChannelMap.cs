using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Card))]
  [KnownType(typeof(Channel))]
  public class ChannelMap
  {
    [DataMember]
    public int ChannelMapId { get; set; }
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public int CardId { get; set; }
    [DataMember]
    public bool EpgOnly { get; set; }

    [DataMember]
    public Card Card { get; set; }
    [DataMember]
    public Channel Channel { get; set; }
  }
}
