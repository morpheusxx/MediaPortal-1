
namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class PersonalTVGuideMap
  {
    public int PersonalTVGuideMapId { get; set; }
    public int KeywordId { get; set; }
    public int ProgramId { get; set; }

    public Program Program { get; set; }
    public Keyword Keyword { get; set; }
  }
}
