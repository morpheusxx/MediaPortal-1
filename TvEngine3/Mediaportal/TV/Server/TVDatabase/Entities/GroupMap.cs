using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(ChannelGroup))]
  [KnownType(typeof(Channel))]
  public class GroupMap
  {
    [DataMember]
    public int GroupMapId { get; set; }
    [DataMember]
    public int ChannelGroupId { get; set; }
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public int SortOrder { get; set; }

    [DataMember]
    public ChannelGroup ChannelGroup { get; set; }
    [DataMember]
    public Channel Channel { get; set; }
  }
}
