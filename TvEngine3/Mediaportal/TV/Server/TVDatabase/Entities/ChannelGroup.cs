using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class ChannelGroup
  {
    public int ChannelGroupId { get; set; }
    public string GroupName { get; set; }
    public int SortOrder { get; set; }
    public int MediaType { get; set; }

    public virtual ICollection<GroupMap> GroupMaps { get; set; }
    public virtual ICollection<KeywordMap> KeywordMap { get; set; }
  }
}
