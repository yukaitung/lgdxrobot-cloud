using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LGDXRobot2Cloud.API.DbContexts
{
  public class LgdxContext : DbContext
  {
    // Navigation
    public DbSet<AutoTask> AutoTasks { get; set; }
    public DbSet<ApiKeyLocation> ApiKeyLocations { get; set; } // Used by Triggers
    public DbSet<Flow> Flows { get; set; }
    public DbSet<FlowDetail> FlowDetails { get; set; }
    public DbSet<Progress> Progresses { get; set; }
    public DbSet<SystemComponent> SystemComponents { get; set; }
    public DbSet<Trigger> Triggers { get; set; }
    public DbSet<Waypoint> Waypoints { get; set; }

    // Robot
    public DbSet<Node> Nodes { get; set; }
    public DbSet<NodesCollection> NodesCollections { get; set; }
    public DbSet<NodesCollectionDetail> NodesCollectionsDetails { get; set; }
    
    public DbSet<Robot> Robots { get; set; }

    public DbSet<RobotSystemInfo> RobotSystemInfos { get; set; }

    // Setting
    public DbSet<ApiKey> ApiKeys { get; set; }

    public LgdxContext(DbContextOptions<LgdxContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
      configurationBuilder
        .Properties<DateTime>()
        .HaveConversion(typeof(UtcValueConverter));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // One AutoTask has many AutoTaskDetails
      modelBuilder.Entity<AutoTask>()
        .HasMany(e => e.Details)
        .WithOne(e => e.AutoTask)
        .HasForeignKey(e => e.AutoTaskId)
        .IsRequired();
      // One Robot has many AutoTasks
      modelBuilder.Entity<Robot>()
        .HasMany(e => e.AssignedTasks)
        .WithOne(e => e.AssignedRobot)
        .HasForeignKey(e => e.AssignedRobotId)
        .IsRequired(false);
      // One Flow has many FlowDetails
      modelBuilder.Entity<Flow>()
        .HasMany(e => e.FlowDetails)
        .WithOne(e => e.Flow)
        .HasForeignKey(e => e.FlowId)
        .IsRequired();
      // One NodesCollection Has many NodesCollectionDetails
      modelBuilder.Entity<NodesCollection>()
        .HasMany(e => e.Nodes)
        .WithOne(e => e.NodesCollection)
        .HasForeignKey(e => e.NodesCollectionId)
        .IsRequired();
      modelBuilder.Entity<Progress>().HasData(
        new Progress
        {
          Id = (int)ProgressState.Saved,
          Name = "Saved",
          System = true,
          Reserved = true
        },
        new Progress
        {
          Id = (int)ProgressState.Waiting,
          Name = "Waiting",
          System = true,
          Reserved = true
        },
        new Progress
        {
          Id = (int)ProgressState.Starting,
          Name = "Starting",
          System = true
        },
        new Progress
        {
          Id = (int)ProgressState.Loading,
          Name = "Loading",
          System = true
        },
        new Progress
        {
          Id = (int)ProgressState.Moving,
          Name = "Moving",
          System = true
        },
        new Progress
        {
          Id = (int)ProgressState.Unloading,
          Name = "Unloading",
          System = true
        },
        new Progress
        {
          Id = (int)ProgressState.Completing,
          Name = "Completing",
          System = true
        },
        new Progress
        {
          Id = (int)ProgressState.Completed,
          Name = "Completed",
          System = true,
          Reserved = true
        },
        new Progress
        {
          Id = (int)ProgressState.Aborted,
          Name = "Aborted",
          System = true,
          Reserved = true
        }
      );
      modelBuilder.Entity<SystemComponent>().HasData(
        new SystemComponent
        {
          Id = (int)SystemComponentName.API,
          Name = "api",
        },
        new SystemComponent
        {
          Id = (int)SystemComponentName.Robot,
          Name = "robot",
        }
      );
      modelBuilder.Entity<ApiKeyLocation>().HasData(
        new ApiKeyLocation
        {
          Id = (int)ApiKeyLocationStr.Header,
          Name = "header",
        },
        new SystemComponent
        {
          Id = (int)ApiKeyLocationStr.Body,
          Name = "body",
        }
      );
      base.OnModelCreating(modelBuilder);
    }
  }

  class UtcValueConverter : ValueConverter<DateTime, DateTime>
  {
    public UtcValueConverter() : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) {}
  }
}