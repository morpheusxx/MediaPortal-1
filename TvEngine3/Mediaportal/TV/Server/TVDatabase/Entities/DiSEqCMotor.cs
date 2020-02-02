using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Card))]
  [KnownType(typeof(Satellite))]
  public class DisEqcMotor
  {
    [DataMember]
    public int DisEqcMotorId { get; set; }
    [DataMember]
    public int CardId { get; set; }
    [DataMember]
    public int SatelliteId { get; set; }
    [DataMember]
    public int Position { get; set; }

    [DataMember]
    public Card Card { get; set; }
    [DataMember]
    public Satellite Satellite { get; set; }
  }
}
