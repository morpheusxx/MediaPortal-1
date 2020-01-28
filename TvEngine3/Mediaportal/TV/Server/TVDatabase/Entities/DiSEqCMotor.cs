namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class DisEqcMotor
  {
    public int DisEqcMotorId { get; set; }
    public int CardId { get; set; }
    public int SatelliteId { get; set; }
    public int Position { get; set; }

    public Card Card { get; set; }
    public Satellite Satellite { get; set; }
  }
}
