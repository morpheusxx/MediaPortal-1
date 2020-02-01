using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class CanceledScheduleManagement
  {
    public static CanceledSchedule SaveCanceledSchedule(CanceledSchedule canceledSchedule)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.CanceledSchedules.Add(canceledSchedule);
        context.SaveChanges();
      }

      ProgramManagement.SynchProgramStates(canceledSchedule.ScheduleId);
      return canceledSchedule;
    }

    public static IList<CanceledSchedule> ListAllCanceledSchedules()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.CanceledSchedules.ToList();
      }
    }
  }
}
