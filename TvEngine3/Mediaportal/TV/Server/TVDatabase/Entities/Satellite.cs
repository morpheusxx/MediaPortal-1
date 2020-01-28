using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Satellite
  {
    public int SatelliteId { get; set; }
    public string SatelliteName { get; set; }
    public string TransponderFileName { get; set; }

    public virtual ICollection<DisEqcMotor> DisEqcMotors { get; set; }
  }
}