using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(DisEqcMotor))]
  public class Satellite
  {
    [DataMember]
    public int SatelliteId { get; set; }
    [DataMember]
    public string SatelliteName { get; set; }
    [DataMember]
    public string TransponderFileName { get; set; }

    [DataMember]
    public virtual ICollection<DisEqcMotor> DisEqcMotors { get; set; }
  }
}