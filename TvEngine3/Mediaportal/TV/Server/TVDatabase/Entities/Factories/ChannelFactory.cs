using System;
using AutoMapper;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class ChannelFactory
  {
    private static readonly Mapper _mapper;
    static ChannelFactory()
    {
      MapperConfiguration config = new MapperConfiguration(c => { c.CreateMap<Schedule, Schedule>(); });
      _mapper = new Mapper(config);
    }
    public static Channel Clone(Channel source)
    {
      return _mapper.Map<Channel>(source);
    }

    public static Channel CreateChannel(MediaTypeEnum mediaType, int timesWatched, DateTime totalTimeWatched, bool grabEpg,
                   DateTime lastGrabTime, int sortOrder, bool visibleInGuide, string externalId,
                   string displayName)
    {
      var channel = new Channel
      {
        MediaType = (int)mediaType,
        TimesWatched = timesWatched,
        TotalTimeWatched = totalTimeWatched,
        GrabEpg = grabEpg,
        LastGrabTime = lastGrabTime,
        SortOrder = sortOrder,
        VisibleInGuide = visibleInGuide,
        ExternalId = externalId,
        DisplayName = displayName
      };
      return channel;
    }

    public static Channel CreateChannel(string name)
    {
      var newChannel = new Channel { VisibleInGuide = true, DisplayName = name };
      return newChannel;
    }
  }
}
