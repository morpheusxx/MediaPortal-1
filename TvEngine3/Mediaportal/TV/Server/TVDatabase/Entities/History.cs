namespace Mediaportal.TV.Server.TVDatabase.Entities
{

  public partial class History
  {
    public int HistoryId { get; set; }
    public int ChannelId { get; set; }
    public System.DateTime StartTime { get; set; }
    public System.DateTime EndTime { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool Recorded { get; set; }
    public int Watched { get; set; }
    public int? ProgramCategoryId { get; set; }

    public Channel Channel { get; set; }
    public ProgramCategory ProgramCategory { get; set; }
  }
}
