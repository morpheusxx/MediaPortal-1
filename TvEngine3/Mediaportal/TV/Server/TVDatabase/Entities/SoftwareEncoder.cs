using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  public class SoftwareEncoder
  {
    [DataMember]
    public int SoftwareEncoderId { get; set; }
    [DataMember]
    public int Priority { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public int Type { get; set; }
    [DataMember]
    public bool Reusable { get; set; }
  }
}
