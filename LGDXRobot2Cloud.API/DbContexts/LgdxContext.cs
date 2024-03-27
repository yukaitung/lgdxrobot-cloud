using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Utilities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.DbContexts
{
  public class LgdxContext : DbContext
  {
    // Navigation
    public DbSet<ApiKeyLocation> ApiKeyLocations { get; set; } // Used by Triggers
    public DbSet<Flow> Flows { get; set; }
    public DbSet<Progress> Progresses { get; set; }
    public DbSet<SystemComponent> SystemComponents { get; set; }
    public DbSet<RobotTask> RobotTasks { get; set; }
    public DbSet<Trigger> Triggers { get; set; }
    public DbSet<Waypoint> Waypoints { get; set; }

    // Robot
    public DbSet<Node> Nodes { get; set; }
    public DbSet<NodesCollection> NodesCollections { get; set; }
    
    public DbSet<Robot> Robots { get; set; }

    // Setting
    public DbSet<ApiKey> ApiKeys { get; set; }

    public LgdxContext(DbContextOptions<LgdxContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // many RobotTasks have many Waypoints
      modelBuilder.Entity<RobotTask>()
        .HasMany(e => e.Waypoints)
        .WithMany(e => e.RobotTasks);
      // many Flows have many many Progress
      modelBuilder.Entity<Flow>()
        .HasMany(e => e.Progresses)
        .WithMany(e => e.Flows);
      // many Flows have many Trigger
      modelBuilder.Entity<Flow>()
        .HasMany(e => e.StartTriggers)
        .WithMany(e => e.Flows)
        .UsingEntity("FlowStartTrigger");
      modelBuilder.Entity<Flow>()
        .HasMany(e => e.EndTriggers)
        .WithMany(e => e.Flows)
        .UsingEntity("FlowEndTrigger");
      modelBuilder.Entity<Progress>().HasData(
        new Progress
        {
          Id = (int)ProgressState.Waiting,
          Name = "Waiting",
          System = true
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
          System = true
        },
        new Progress
        {
          Id = (int)ProgressState.Aborted,
          Name = "Aborted",
          System = true
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
      base.OnModelCreating(modelBuilder);
    }
  }
}