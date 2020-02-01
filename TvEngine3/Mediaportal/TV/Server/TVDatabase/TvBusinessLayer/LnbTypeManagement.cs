using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class LnbTypeManagement
  {
    public static LnbType GetLnbType(int idLnbType)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.LnbTypes.FirstOrDefault(l => l.LnbTypeId == idLnbType);
      }
    }

    public static IList<LnbType> ListAllLnbTypes()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.LnbTypes.ToList();
      }
    }
  }
}
