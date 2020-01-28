namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class KeywordMap
  {
    public int KeywordMapId { get; set; }
    public int KeywordId { get; set; }

    public int ChannelGroupId { get; set; }
    public Keyword Keyword { get; set; }
    public ChannelGroup ChannelGroups { get; set; }
  }
}
