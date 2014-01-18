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
    public NowAndNext()
    {
    }

    public NowAndNext(int idChannel, DateTime startTimeNowStart, DateTime endTimeNowEnd, string titleNow, string titleNext,
                      int idProgramNow, int idProgramNext,
                      string episodeNameNow, string episodeNameNext, string seriesNumNow, string seriesNumNext,
                      string episodeNumNow, string episodeNumNext, string episodePartNow, string episodePartNext)
    {
      IdChannel = idChannel;
      StartTimeNow = startTimeNowStart;
      EndTimeNow = endTimeNowEnd;
      TitleNow = titleNow;
      TitleNext = titleNext;
      IdProgramNow = idProgramNow;
      IdProgramNext = idProgramNext;
      EpisodeNameNow = episodeNameNow;
      EpisodeNameNext = episodeNameNext;
      SeriesNumNow = seriesNumNow;
      SeriesNumNext = seriesNumNext;
      EpisodeNumNow = episodeNumNow;
      EpisodeNumNext = episodeNumNext;
      EpisodePartNow = episodePartNow;
      EpisodePartNext = episodePartNext;
    }

    [DataMember]
    public int IdChannel { get; set; }

    [DataMember]
    public DateTime StartTimeNow { get; set; }

    [DataMember]
    public DateTime StartTimeNext { get; set; }

    [DataMember]
    public DateTime EndTimeNow { get; set; }

    [DataMember]
    public DateTime EndTimeNext { get; set; }

    [DataMember]
    public string TitleNow { get; set; }

    [DataMember]
    public string TitleNext { get; set; }

    [DataMember]
    public string DescriptionNow { get; set; }

    [DataMember]
    public string DescriptionNext { get; set; }

    [DataMember]
    public int IdProgramNow { get; set; }

    [DataMember]
    public int IdProgramNext { get; set; }

    [DataMember]
    public string EpisodeNameNow { get; set; }

    [DataMember]
    public string EpisodeNameNext { get; set; }

    [DataMember]
    public string SeriesNumNow { get; set; }

    [DataMember]
    public string SeriesNumNext { get; set; }

    [DataMember]
    public string EpisodeNumNow { get; set; }

    [DataMember]
    public string EpisodeNumNext { get; set; }

    [DataMember]
    public string EpisodePartNow { get; set; }

    [DataMember]
    public string EpisodePartNext { get; set; }
  }
}