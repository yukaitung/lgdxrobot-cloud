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
    public DbSet<SignalEmitter> SignalEmitters { get; set; }
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
  }
}