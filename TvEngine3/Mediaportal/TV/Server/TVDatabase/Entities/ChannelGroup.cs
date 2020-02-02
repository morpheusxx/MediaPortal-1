using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(GroupMap))]
  [KnownType(typeof(KeywordMap))]
  public class ChannelGroup
  {
    [DataMember] public int ChannelGroupId { get; set; }
    public string GroupName { get; set; }
    [DataMember]
    public int SortOrder { get; set; }
    [DataMember]
    public int MediaType { get; set; }

    [DataMember]
    public virtual ICollection<GroupMap> GroupMaps { get; set; }
    [DataMember]
    public virtual ICollection<KeywordMap> KeywordMap { get; set; }
  }
}
