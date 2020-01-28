namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class ChannelMap
  {
    public int ChannelMapId { get; set; }
    public int ChannelId { get; set; }
    public int CardId { get; set; }
    public bool EpgOnly { get; set; }

    public Card Card { get; set; }
    public Channel Channel { get; set; }
  }
}
