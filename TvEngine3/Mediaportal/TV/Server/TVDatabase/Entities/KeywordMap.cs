using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Keyword))]
  [KnownType(typeof(ChannelGroup))]
  public class KeywordMap
  {
    [DataMember]
    public int KeywordMapId { get; set; }
    [DataMember]
    public int KeywordId { get; set; }

    [DataMember]
    public int ChannelGroupId { get; set; }
    [DataMember]
    public Keyword Keyword { get; set; }
    [DataMember]
    public ChannelGroup ChannelGroups { get; set; }
  }
}
