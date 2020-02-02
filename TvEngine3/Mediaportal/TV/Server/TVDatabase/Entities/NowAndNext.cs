#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract]
  public class NowAndNext
  {
    public NowAndNext(int channelId, DateTime nowStart, DateTime nowEnd, string titleNow, string titleNext,
                      int programNowId, int programNextId,
                      string episodeName, string episodeNameNext, string seriesNum, string seriesNumNext,
                      string episodeNum, string episodeNumNext, string episodePart, string episodePartNext)
    {
      ChannelId = channelId;
      NowStartTime = nowStart;
      NowEndTime = nowEnd;
      TitleNow = titleNow;
      TitleNext = titleNext;
      ProgramNowId = programNowId;
      ProgramNextId = programNextId;
      EpisodeName = episodeName;
      EpisodeNameNext = episodeNameNext;
      SeriesNum = seriesNum;
      SeriesNumNext = seriesNumNext;
      EpisodeNum = episodeNum;
      EpisodeNumNext = episodeNumNext;
      EpisodePart = episodePart;
      EpisodePartNext = episodePartNext;
    }

    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public DateTime NowStartTime { get; set; }
    [DataMember]
    public DateTime NowEndTime { get; set; }
    [DataMember]
    public string TitleNow { get; set; }
    [DataMember]
    public string TitleNext { get; set; }
    [DataMember]
    public int ProgramNowId { get; set; }
    [DataMember]
    public int ProgramNextId { get; set; }
    [DataMember]
    public string EpisodeName { get; set; }
    [DataMember]
    public string EpisodeNameNext { get; set; }
    [DataMember]
    public string SeriesNum { get; set; }
    [DataMember]
    public string SeriesNumNext { get; set; }
    [DataMember]
    public string EpisodeNum { get; set; }
    [DataMember]
    public string EpisodeNumNext { get; set; }
    [DataMember]
    public string EpisodePart { get; set; }
    [DataMember]
    public string EpisodePartNext { get; set; }
  }
}