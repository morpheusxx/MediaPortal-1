namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class ProgramCredit
  {
    public int ProgramCreditId { get; set; }
    public int ProgramId { get; set; }
    public string Person { get; set; }
    public string Role { get; set; }

    public Program Program { get; set; }

    public override string ToString()
    {
      return ("[" + Role + "] = [" + Person + "]");
    }
  }
}
