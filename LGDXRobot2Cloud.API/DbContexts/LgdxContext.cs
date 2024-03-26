using LGDXRobot2Cloud.API.Entities;
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
    public DbSet<Entities.Task> Tasks { get; set; }
    public DbSet<Trigger> Triggers { get; set; }
    public DbSet<Waypoint> Waypoints { get; set; }

    // Robot
    public DbSet<Node> Nodes { get; set; }
    public DbSet<NodesComposition> NodesCompositions { get; set; }
    
    public DbSet<Robot> Robots { get; set; }

    // Setting
    public DbSet<ApiKey> ApiKeys { get; set; }

    public LgdxContext(DbContextOptions<LgdxContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // many Tasks have many Waypoints
      modelBuilder.Entity<Entities.Task>()
        .HasMany(e => e.Waypoints)
        .WithMany(e => e.Tasks);
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
          Id = 1,
          Name = "Waiting",
          System = true
        },
        new Progress
        {
          Id = 2,
          Name = "Starting",
          System = true
        },
        new Progress
        {
          Id = 3,
          Name = "Loading",
          System = true
        },
        new Progress
        {
          Id = 4,
          Name = "Moving",
          System = true
        },
        new Progress
        {
          Id = 5,
          Name = "Unloading",
          System = true
        },
        new Progress
        {
          Id = 6,
          Name = "Completing",
          System = true
        },
        new Progress
        {
          Id = 7,
          Name = "Completed",
          System = true
        },
        new Progress
        {
          Id = 8,
          Name = "Aborted",
          System = true
        }
      );
      modelBuilder.Entity<SystemComponent>().HasData(
        new SystemComponent
        {
          Id = 1,
          Name = "api",
        },
        new SystemComponent
        {
          Id = 2,
          Name = "robot",
        }
      );
      base.OnModelCreating(modelBuilder);
    }
  }
}