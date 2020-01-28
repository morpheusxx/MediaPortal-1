namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class RecordingCredit
  {
    public int RecordingCreditId { get; set; }
    public int RecordingId { get; set; }
    public string Person { get; set; }
    public string Role { get; set; }

    public Recording Recording { get; set; }
  }
}
