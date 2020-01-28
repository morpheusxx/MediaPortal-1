namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class CardGroupMap
  {
    public int CardGroupMapId { get; set; }
    public int CardId { get; set; }
    public int CardGroupId { get; set; }

    public Card Card { get; set; }
    public CardGroup CardGroup { get; set; }
  }
}
