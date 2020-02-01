using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl
{
  [DataContract]
  public class SubChannel : ISubChannel
  {
    [DataMember]
    private int _id;

    [DataMember]
    private int _channelId;

    [DataMember]
    private TvUsage _tvUsage;

    public SubChannel()
    {
    }

    public SubChannel(int id, int channelId, TvUsage tvUsage)
    {
      _id = id;
      _channelId = channelId;
      _tvUsage = tvUsage;
    }

    public int ChannelId
    {
      get { return _channelId; }
      set { _channelId = value; }
    }

    public int Id
    {
      get { return _id; }
      set { _id = value; }
    }

    public TvUsage TvUsage
    {
      get { return _tvUsage; }
      set { _tvUsage = value; }
    }
  }
}