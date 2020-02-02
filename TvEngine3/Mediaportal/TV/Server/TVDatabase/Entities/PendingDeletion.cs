using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  public class PendingDeletion
  {
    [DataMember]
    public int PendingDeletionId { get; set; }
    [DataMember]
    public string FileName { get; set; }
  }
}
