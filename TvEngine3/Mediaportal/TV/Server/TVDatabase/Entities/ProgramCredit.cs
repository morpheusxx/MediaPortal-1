using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Program))]
  public class ProgramCredit
  {
    [DataMember]
    public int ProgramCreditId { get; set; }
    [DataMember]
    public int ProgramId { get; set; }
    [DataMember]
    public string Person { get; set; }
    [DataMember]
    public string Role { get; set; }

    [DataMember]
    public Program Program { get; set; }

    public override string ToString()
    {
      return ("[" + Role + "] = [" + Person + "]");
    }
  }
}
