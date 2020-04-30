﻿using System;
using AutoMapper;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class ProgramFactory
  {
    private static readonly Mapper _mapper;

    static ProgramFactory()
    {
      MapperConfiguration config = new MapperConfiguration(c => { c.CreateMap<Program, Program>(); });
      _mapper = new Mapper(config);
    }

    public static Program Clone(Program source)
    {
      var clone = _mapper.Map<Program>(source);
      // Clear primary key, so EF core creates a new key when saving. Otherwise you get unique constraint violations.
      clone.ProgramId = 0;
      return clone;
    }

    public static Program CreateProgram(int idChannel, DateTime startTime, DateTime endTime, string title, string description, ProgramCategory category,
                   ProgramState state, DateTime originalAirDate, string seriesNum, string episodeNum, string episodeName,
                   string episodePart, int starRating,
                   string classification, int parentalRating)
    {
      //todo gibman handle genre
      var program = new Program
      {
        ChannelId = idChannel,
        StartTime = startTime,
        EndTime = endTime,
        Title = title,
        Description = description,
        State = (int)state,
        OriginalAirDate = originalAirDate,
        SeriesNum = seriesNum,
        EpisodeNum = episodeNum,
        EpisodeName = episodeName,
        EpisodePart = episodePart,
        StarRating = starRating,
        Classification = classification,
        ParentalRating = parentalRating,
      };
      // Note: do not assign the ProgramCategory objects here, as EF state tracking would lead to duplicated programs,
      // as also the relation to ProgramCategory will be tracked.
      if (category != null)
      {
        program.ProgramCategoryId = category.ProgramCategoryId;
      }
      return program;
    }

    /*
     public static Program CreateProgram (int idChannel, DateTime startTime, DateTime endTime, string title, string description,
                    ProgramState state, DateTime originalAirDate, string seriesNum, string episodeNum, string episodeName,
                    string episodePart, int starRating,
                    string classification, int parentalRating)
     {
       var program = new Program();
       return program;

       this.idChannel = idChannel;
       this.startTime = startTime;
       this.endTime = endTime;
       this.title = title;
       this.description = description;
       this.state = (int)state;
       this.originalAirDate = originalAirDate;
       this.seriesNum = seriesNum;
       this.episodeNum = episodeNum;
       this.episodeName = episodeName;
       this.episodePart = episodePart;
       this.starRating = starRating;
       this.classification = classification;
       this.parentalRating = parentalRating;
     }


     public static Program CreateProgram (int idProgram, int idChannel, DateTime startTime, DateTime endTime, string title, string description,
                    string genre, ProgramState state, DateTime originalAirDate, string seriesNum, string episodeNum,
                    string episodeName, string episodePart,
                    int starRating, string classification, int parentalRating)
     {
       var program = new Program();
       return program; 
       this.idProgram = idProgram;
       this.idChannel = idChannel;
       this.startTime = startTime;
       this.endTime = endTime;
       this.title = title;
       this.description = description;
       this.state = (int)state;
       this.originalAirDate = originalAirDate;
       this.seriesNum = seriesNum;
       this.episodeNum = episodeNum;
       this.episodeName = episodeName;
       this.episodePart = episodePart;
       this.starRating = starRating;
       this.classification = classification;
       this.parentalRating = parentalRating;
     }

     public static Program CreateProgram (int idProgram, int idChannel, DateTime startTime, DateTime endTime, string title, string description,
                    ProgramState state, DateTime originalAirDate, string seriesNum, string episodeNum,
                    string episodeName, string episodePart,
                    int starRating, string classification, int parentalRating)
     {
       var program = new Program();
       return program;
       this.idProgram = idProgram;
       this.idChannel = idChannel;
       this.startTime = startTime;
       this.endTime = endTime;
       this.title = title;
       this.description = description;
       this.state = (int)state;
       this.originalAirDate = originalAirDate;
       this.seriesNum = seriesNum;
       this.episodeNum = episodeNum;
       this.episodeName = episodeName;
       this.episodePart = episodePart;
       this.starRating = starRating;
       this.classification = classification;
       this.parentalRating = parentalRating;
     }
   */



  }
}
