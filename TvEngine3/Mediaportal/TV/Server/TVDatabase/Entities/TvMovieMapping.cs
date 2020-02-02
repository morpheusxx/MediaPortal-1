using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Channel))]
  public class TvMovieMapping
  {
    [DataMember]
    public int TvMovieMappingId { get; set; }
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public string StationName { get; set; }
    [DataMember]
    public string TimeSharingStart { get; set; }
    [DataMember]
    public string TimeSharingEnd { get; set; }

    [DataMember]
    public Channel Channel { get; set; }
  }
}
