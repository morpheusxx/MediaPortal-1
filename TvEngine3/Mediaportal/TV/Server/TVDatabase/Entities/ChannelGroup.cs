using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(GroupMap))]
  [KnownType(typeof(KeywordMap))]
  public class ChannelGroup
  {
    [DataMember]
    public int ChannelGroupId { get; set; }
    [DataMember]
    public string GroupName { get; set; }
    [DataMember]
    public int SortOrder { get; set; }
    [DataMember]
    public int MediaType { get; set; }

    [DataMember]
    public virtual List<GroupMap> GroupMaps { get; set; } = new List<GroupMap>();
    [DataMember]
    public virtual List<KeywordMap> KeywordMap { get; set; } = new List<KeywordMap>();
  }
}
