using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  public class Favorite
  {
    [DataMember]
    public int FavoriteId { get; set; }
    [DataMember]
    public int ProgramId { get; set; }
    [DataMember]
    public int Priority { get; set; }
    [DataMember]
    public int TimesWatched { get; set; }
  }
}
