namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Favorite
  {
    public int FavoriteId { get; set; }
    public int ProgramId { get; set; }
    public int Priority { get; set; }
    public int TimesWatched { get; set; }
  }
}
