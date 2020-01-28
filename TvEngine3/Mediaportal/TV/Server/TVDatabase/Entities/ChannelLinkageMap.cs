namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class ChannelLinkageMap
  {
    public int ChannelLinkageMapId { get; set; }
    public int PortalChannelId { get; set; }
    public int LinkedChannelId { get; set; }
    public string DisplayName { get; set; }

    public Channel LinkedChannel { get; set; }
    public Channel PortalChannel { get; set; }
  }
}
