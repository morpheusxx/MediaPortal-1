using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class CardGroup
  {
    public int CardGroupId { get; set; }
    public string Name { get; set; }
    public virtual ICollection<CardGroupMap> CardGroupMaps { get; set; }
  }
}
