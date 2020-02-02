using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ConflictManagement
  {
    public static IList<Conflict> ListAllConflicts()
    {
      using (var context = new TvEngineDbContext())
      {
        return context.Conflicts.ToList();
      }
    }

    public static Conflict SaveConflict(Conflict conflict)
    {
      using (var context = new TvEngineDbContext())
      {
        context.Conflicts.Add(conflict);
        context.SaveChanges();
        return conflict;
      }
    }

    public static Conflict GetConflict(int idConflict)
    {
      using (var context = new TvEngineDbContext())
      {
        return context.Conflicts.FirstOrDefault(s => s.ConflictId == idConflict);
      }
    }

    public static void DeleteConflict(int idConflict)
    {
      using (var context = new TvEngineDbContext())
      {
        var conflict = context.Conflicts.FirstOrDefault(c => c.ConflictId == idConflict);
        if (conflict != null)
        {
          context.Conflicts.Remove(conflict);
          context.SaveChanges(true);
        }
      }
    }
  }
}
