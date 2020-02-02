using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  public class ScheduleRulesTemplate
  {
    [DataMember]
    public int ScheduleRulesTemplateId { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public string Rules { get; set; }
    [DataMember]
    public bool Enabled { get; set; }
    [DataMember]
    public int Usages { get; set; }
    [DataMember]
    public bool Editable { get; set; }
  }
}
