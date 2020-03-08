using Mediaportal.TV.Server.TVDatabase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Context
{
  public class SeedData
  {
    public static void EnsureSeedData(TvEngineDbContext ctx)
    {
      if (ctx.LnbTypes.Any())
        return;

      // Enable write-ahead logging
      ctx.Database.OpenConnection();
      var cmd = ctx.Database.GetDbConnection().CreateCommand();
      cmd.CommandText = @"PRAGMA journal_mode = 'wal'";
      cmd.ExecuteNonQuery();

      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 1, Name = "Universal", LowBandFrequency = 9750000, HighBandFrequency = 10600000, SwitchFrequency = 11700000, IsBandStacked = false, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 2, Name = "C-Band", LowBandFrequency = 5150000, HighBandFrequency = 5650000, SwitchFrequency = 18000000, IsBandStacked = false, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 3, Name = "10700 MHz", LowBandFrequency = 10700000, HighBandFrequency = 11200000, SwitchFrequency = 18000000, IsBandStacked = false, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 4, Name = "10750 MHz", LowBandFrequency = 10750000, HighBandFrequency = 11250000, SwitchFrequency = 18000000, IsBandStacked = false, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 5, Name = "11250 MHz (NA Legacy)", LowBandFrequency = 11250000, HighBandFrequency = 11750000, SwitchFrequency = 18000000, IsBandStacked = false, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 6, Name = "11300 MHz", LowBandFrequency = 11300000, HighBandFrequency = 11800000, SwitchFrequency = 18000000, IsBandStacked = false, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 7, Name = "DishPro Band Stacked FSS", LowBandFrequency = 10750000, HighBandFrequency = 13850000, SwitchFrequency = 18000000, IsBandStacked = true, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 8, Name = "DishPro Band Stacked DBS", LowBandFrequency = 11250000, HighBandFrequency = 14350000, SwitchFrequency = 18000000, IsBandStacked = true, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 9, Name = "NA Band Stacked FSS", LowBandFrequency = 10750000, HighBandFrequency = 10175000, SwitchFrequency = 18000000, IsBandStacked = true, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 10, Name = "NA Band Stacked DBS", LowBandFrequency = 11250000, HighBandFrequency = 10675000, SwitchFrequency = 18000000, IsBandStacked = true, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 11, Name = "Sadoun Band Stacked", LowBandFrequency = 10100000, HighBandFrequency = 10750000, SwitchFrequency = 18000000, IsBandStacked = true, IsToroidal = false });
      ctx.LnbTypes.Add(new LnbType { LnbTypeId = 12, Name = "C-Band Band Stacked", LowBandFrequency = 5150000, HighBandFrequency = 5750000, SwitchFrequency = 18000000, IsBandStacked = true, IsToroidal = false });

      // List of video encoders
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 1, Name = "InterVideo Video Encoder", Priority = 1, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 2, Name = "CyberLink MPEG Video Encoder", Priority = 2, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 3, Name = "CyberLink MPEG Video Encoder(KWorld)", Priority = 3, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 4, Name = "CyberLink MPEG Video Encoder(TerraTec)", Priority = 4, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 5, Name = "CyberLink MPEG Video Encoder(Twinhan)", Priority = 5, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 6, Name = "ATI MPEG Video Encoder", Priority = 6, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 7, Name = "MainConcept MPEG Video Encoder", Priority = 7, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 8, Name = "MainConcept Demo MPEG Video Encoder", Priority = 8, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 9, Name = "MainConcept (Hauppauge) MPEG Video Encoder", Priority = 9, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 10, Name = "MainConcept (HCW) MPEG-2 Video Encoder", Priority = 10, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 11, Name = "Pinnacle MPEG 2 Encoder", Priority = 11, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 12, Name = "nanocosmos MPEG Video Encoder", Priority = 12, Reusable = true, Type = 0 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 13, Name = "Ulead MPEG Encoder", Priority = 13, Reusable = true, Type = 0 });

      // List of audio encoders
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 14, Name = "InterVideo Audio Encoder", Priority = 1, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 15, Name = "CyberLink Audio Encoder", Priority = 2, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 16, Name = "CyberLink MPEG Audio Encoder", Priority = 3, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 17, Name = "CyberLink Audio Encoder(KWorld)", Priority = 4, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 18, Name = "CyberLink Audio Encoder(TechnoTrend)", Priority = 5, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 19, Name = "CyberLink Audio Encoder(TerraTec)", Priority = 6, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 20, Name = "CyberLink Audio Encoder(Twinhan)", Priority = 7, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 21, Name = "ATI MPEG Audio Encoder", Priority = 8, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 22, Name = "MainConcept MPEG Audio Encoder", Priority = 9, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 23, Name = "MainConcept Demo MPEG Audio Encoder", Priority = 10, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 24, Name = "MainConcept (Hauppauge) MPEG Audio Encoder", Priority = 11, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 25, Name = "MainConcept (HCW) Layer II Audio Encoder", Priority = 12, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 26, Name = "Pinnacle MPEG Layer-2 Audio Encoder", Priority = 13, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 27, Name = "NVIDIA Audio Encoder", Priority = 14, Reusable = true, Type = 1 });
      ctx.SoftwareEncoders.Add(new SoftwareEncoder { SoftwareEncoderId = 28, Name = "Ulead MPEG Audio Encoder", Priority = 15, Reusable = true, Type = 1 });

      ctx.SaveChanges(true);
      ctx.Database.CloseConnection();
    }
  }
}