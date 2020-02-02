using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Extensions;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ProgramCategoryManagement
  {

    public static void AddCategory(ProgramCategory category)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(category);
        context.SaveChanges(true);
      }
    }

    public static IList<ProgramCategory> ListAllProgramCategories()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.ProgramCategories.IncludeAllRelations().ToList();
      }
    }

    public static ProgramCategory SaveProgramCategory(ProgramCategory category)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(category);
        context.SaveChanges(true);
        return category;
      }
    }

    public static TvGuideCategory AddTvGuideCategory(TvGuideCategory tvGuideCategorycategory)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        context.Update(tvGuideCategorycategory);
        context.SaveChanges(true);
        return tvGuideCategorycategory;
      }
    }

    public static IList<TvGuideCategory> ListAllTvGuideCategories()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.TvGuideCategories.ToList();
      }
    }
  }
}
