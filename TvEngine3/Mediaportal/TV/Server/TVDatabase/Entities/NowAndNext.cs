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

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
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

    public int ChannelId { get; set; }
    public DateTime NowStartTime { get; set; }
    public DateTime NowEndTime { get; set; }
    public string TitleNow { get; set; }
    public string TitleNext { get; set; }
    public int ProgramNowId { get; set; }
    public int ProgramNextId { get; set; }
    public string EpisodeName { get; set; }
    public string EpisodeNameNext { get; set; }
    public string SeriesNum { get; set; }
    public string SeriesNumNext { get; set; }
    public string EpisodeNum { get; set; }
    public string EpisodeNumNext { get; set; }
    public string EpisodePart { get; set; }
    public string EpisodePartNext { get; set; }
  }
}