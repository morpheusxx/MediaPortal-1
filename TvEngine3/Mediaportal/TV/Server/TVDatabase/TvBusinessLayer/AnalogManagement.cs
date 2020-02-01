using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class AnalogManagement
  {
    #region SoftwareEncoders

    public static IList<SoftwareEncoder> GetSoftwareEncodersVideo()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.SoftwareEncoders.Where(s => s.Type == 0).OrderBy(s => s.Priority).ToList();
      }
    }

    public static IList<SoftwareEncoder> GetSoftwareEncodersAudio()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        return context.SoftwareEncoders.Where(s => s.Type == 1).OrderBy(s => s.Priority).ToList();
      }
    }

    #endregion
  }
}
