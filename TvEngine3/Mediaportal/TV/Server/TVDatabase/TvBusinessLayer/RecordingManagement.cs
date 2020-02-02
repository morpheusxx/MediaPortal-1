using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class RecordingManagement
  {
    public static Recording GetRecording(int idRecording)
    {
      //lazy loading verified ok
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        Recording recording = context.Recordings.Include(r => r.Channel)
          .Include(r => r.RecordingCredits)
          .Include(r => r.Schedule)
          .Include(r => r.ProgramCategory)
          .FirstOrDefault(r => r.RecordingId == idRecording);
        return recording;
      }
    }

    public static void DeleteRecording(int idRecording)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        Recording recording = context.Recordings.FirstOrDefault(r => r.RecordingId == idRecording);
        if (recording != null)
        {
          context.Recordings.Remove(recording);
          context.SaveChanges(true);
        }
      }
    }

    public static IList<Recording> ListAllRecordingsByMediaType(MediaTypeEnum mediaType)
    {
      //lazy loading verified ok
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Recordings
          .Include(r => r.Channel)
          .Include(r => r.RecordingCredits)
          .Include(c => c.Schedule)
          .Include(r => r.ProgramCategory)
          .Where(r => r.MediaType == (int)mediaType)
          .ToList();
      }
    }

    public static Recording SaveRecording(Recording recording)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(recording);
        context.SaveChanges(true);
        return recording;
      }
    }

    public static Recording GetRecordingByFileName(string filename)
    {
      //lazy loading verified ok
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Recordings.IncludeAllRelations().FirstOrDefault(r => r.FileName == filename);
      }
    }

    public static Recording GetActiveRecording(int idSchedule)
    {
      //lazy loading verified ok
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Recordings.IncludeAllRelations().FirstOrDefault(r => r.IsRecording && r.ScheduleId == idSchedule);
      }
    }

    public static Recording GetActiveRecordingByTitleAndChannel(string title, int idChannel)
    {
      //lazy loading verified ok
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Recordings.IncludeAllRelations().FirstOrDefault(r => r.IsRecording && r.ChannelId == idChannel && r.Title == title);
      }
    }

    public static IList<Recording> ListAllActiveRecordingsByMediaType(MediaTypeEnum mediaType)
    {
      //lazy loading verified ok
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.Recordings.IncludeAllRelations().Where(c => c.MediaType == (int)mediaType && c.IsRecording).ToList();
      }
    }

    public static bool HasRecordingPendingDeletion(string filename)
    {
      bool hasRecordingPendingDeletion;
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        hasRecordingPendingDeletion = context.PendingDeletions.Any(c => c.FileName == filename);
      }
      return hasRecordingPendingDeletion;
    }

    public static PendingDeletion SaveRecordingPendingDeletion(PendingDeletion pendingDeletion)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(pendingDeletion);
        context.SaveChanges(true);
        return pendingDeletion;
      }
    }

    public static void DeletePendingRecordingDeletion(int idPendingDeletion)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var pendingDeletion = context.PendingDeletions.FirstOrDefault(s => s.PendingDeletionId == idPendingDeletion);
        if (pendingDeletion != null)
        {
          context.PendingDeletions.Remove(pendingDeletion);
          context.SaveChanges(true);
        }
      }
    }

    public static PendingDeletion GetPendingRecordingDeletion(int idPendingDeletion)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.PendingDeletions.FirstOrDefault(p => p.PendingDeletionId == idPendingDeletion);
      }
    }

    public static IList<PendingDeletion> ListAllPendingRecordingDeletions()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.PendingDeletions.ToList();
      }
    }

    public static void ResetActiveRecordings()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        foreach (Recording rec in context.Recordings)
        {
          rec.IsRecording = false;
        }
        context.SaveChanges(true);
      }
    }
  }
}
