namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class TvMovieMapping
  {
    public int TvMovieMappingId { get; set; }
    public int ChannelId { get; set; }
    public string StationName { get; set; }
    public string TimeSharingStart { get; set; }
    public string TimeSharingEnd { get; set; }

    public Channel Channel { get; set; }
  }
}
