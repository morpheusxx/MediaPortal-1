namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class GroupMap
  {
    public int GroupMapId { get; set; }
    public int ChannelGroupId { get; set; }
    public int ChannelId { get; set; }
    public int SortOrder { get; set; }

    public ChannelGroup ChannelGroup { get; set; }
    public Channel Channel { get; set; }
  }
}
