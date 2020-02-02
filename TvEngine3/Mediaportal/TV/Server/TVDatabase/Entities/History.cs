using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Channel))]
  [KnownType(typeof(ProgramCategory))]
  public class History
  {
    [DataMember]
    public int HistoryId { get; set; }
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public System.DateTime StartTime { get; set; }
    [DataMember]
    public System.DateTime EndTime { get; set; }
    [DataMember]
    public string Title { get; set; }
    [DataMember]
    public string Description { get; set; }
    [DataMember]
    public bool Recorded { get; set; }
    [DataMember]
    public int Watched { get; set; }
    [DataMember]
    public int? ProgramCategoryId { get; set; }

    [DataMember]
    public Channel Channel { get; set; }
    [DataMember]
    public ProgramCategory ProgramCategory { get; set; }
  }
}
