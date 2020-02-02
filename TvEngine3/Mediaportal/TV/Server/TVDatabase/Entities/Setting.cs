using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  public class Setting
  {
    [DataMember]
    public int SettingId { get; set; }
    [DataMember]
    public string Tag { get; set; }
    [DataMember]
    public string Value { get; set; }
  }
}
