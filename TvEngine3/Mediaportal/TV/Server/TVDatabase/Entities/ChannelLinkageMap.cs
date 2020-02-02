using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Channel))]
  public class ChannelLinkageMap
  {
    [DataMember]
    public int ChannelLinkageMapId { get; set; }
    [DataMember]
    public int PortalChannelId { get; set; }
    [DataMember]
    public int LinkedChannelId { get; set; }
    [DataMember]
    public string DisplayName { get; set; }

    [DataMember]
    public Channel LinkedChannel { get; set; }
    [DataMember]
    public Channel PortalChannel { get; set; }
  }
}
