using System.Configuration;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Context
{
  public class TvEngineDbContext : DbContext
  {
    public DbSet<CanceledSchedule> CanceledSchedules { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardGroup> CardGroups { get; set; }
    public DbSet<CardGroupMap> CardGroupMaps { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<ChannelGroup> ChannelGroups { get; set; }
    public DbSet<ChannelLinkageMap> ChannelLinkageMaps { get; set; }
    public DbSet<ChannelMap> ChannelMaps { get; set; }
    public DbSet<Conflict> Conflicts { get; set; }
    public DbSet<DisEqcMotor> DisEqcMotors { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<GroupMap> GroupMaps { get; set; }
    public DbSet<History> Histories { get; set; }
    public DbSet<Keyword> Keywords { get; set; }
    public DbSet<KeywordMap> KeywordMaps { get; set; }
    public DbSet<LnbType> LnbTypes { get; set; }
    public DbSet<PendingDeletion> PendingDeletions { get; set; }
    public DbSet<PersonalTVGuideMap> PersonalTVGuideMaps { get; set; }
    public DbSet<Mediaportal.TV.Server.TVDatabase.Entities.Program> Programs { get; set; }
    public DbSet<ProgramCategory> ProgramCategories { get; set; }
    public DbSet<ProgramCredit> ProgramCredits { get; set; }
    public DbSet<Recording> Recordings { get; set; }
    public DbSet<RecordingCredit> RecordingCredits { get; set; }
    public DbSet<RuleBasedSchedule> RuleBasedSchedules { get; set; }
    public DbSet<Satellite> Satellites { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<ScheduleRulesTemplate> ScheduleRulesTemplates { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<SoftwareEncoder> SoftwareEncoders { get; set; }
    public DbSet<Timespan> Timespans { get; set; }
    public DbSet<TuningDetail> TuningDetails { get; set; }
    public DbSet<TvGuideCategory> TvGuideCategories { get; set; }
    public DbSet<TvMovieMapping> TvMovieMappings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<ChannelLinkageMap>()
        .HasOne(p => p.PortalChannel)
        .WithMany(b => b.ChannelPortalMaps)
        .HasForeignKey(p => p.PortalChannelId);

      modelBuilder.Entity<ChannelLinkageMap>()
        .HasOne(p => p.LinkedChannel)
        .WithMany(b => b.ChannelLinkMaps)
        .HasForeignKey(p => p.LinkedChannelId);

      modelBuilder.Entity<Conflict>()
        .HasOne(p => p.Schedule)
        .WithMany(b => b.Conflicts);

      modelBuilder.Entity<Recording>()
       .HasOne(r => r.Channel)
       .WithMany(c => c.Recordings)
       .OnDelete(DeleteBehavior.SetNull);

      modelBuilder.Entity<Recording>()
       .HasOne(r => r.ProgramCategory)
       .WithMany(c => c.Recordings)
       .OnDelete(DeleteBehavior.SetNull);

      modelBuilder.Entity<Recording>()
       .HasOne(r => r.Schedule)
       .WithMany(c => c.Recordings)
       .OnDelete(DeleteBehavior.SetNull);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
      var container = Instantiator.Instance.Container();
      ConnectionStringSettings connection = container.Resolve<ConnectionStringSettings>();
      if (connection.ProviderName == "SQLite")
      {
        options.UseSqlite(connection.ConnectionString);
      }
    }
  }

  public static class DbSetup
  {
    private static readonly object _syncObj = new object();
    public static bool EnsureCreated()
    {
      lock (_syncObj)
        using (var context = new TvEngineDbContext())
        {
          context.Database.EnsureCreated();
          SeedData.EnsureSeedData(context);
          context.SaveChanges();
        }

      return true;
    }
  }
}
