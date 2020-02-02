using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Recording))]
  public class RecordingCredit
  {
    [DataMember]
    public int RecordingCreditId { get; set; }
    [DataMember]
    public int RecordingId { get; set; }
    [DataMember]
    public string Person { get; set; }
    [DataMember]
    public string Role { get; set; }

    [DataMember]
    public Recording Recording { get; set; }
  }
}
