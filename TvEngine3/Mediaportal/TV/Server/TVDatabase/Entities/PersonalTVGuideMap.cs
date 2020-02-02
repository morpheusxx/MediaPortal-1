
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Program))]
  [KnownType(typeof(Keyword))]
  public class PersonalTVGuideMap
  {
    [DataMember]
    public int PersonalTVGuideMapId { get; set; }
    [DataMember]
    public int KeywordId { get; set; }
    [DataMember]
    public int ProgramId { get; set; }

    [DataMember]
    public Program Program { get; set; }
    [DataMember]
    public Keyword Keyword { get; set; }
  }
}
