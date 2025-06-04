using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LGDXRobotCloud.Data.DbContexts;

public class LgdxContext(DbContextOptions<LgdxContext> options) : IdentityDbContext<LgdxUser, LgdxRole, string>(options)
{
  // Administration
  public virtual DbSet<ApiKey> ApiKeys { get; set; }

  // Automation
  public virtual DbSet<AutoTask> AutoTasks { get; set; }
  public virtual DbSet<AutoTaskDetail> AutoTasksDetail { get; set; }
  public virtual DbSet<AutoTaskJourney> AutoTasksJourney { get; set; }
  public virtual DbSet<Flow> Flows { get; set; }
  public virtual DbSet<FlowDetail> FlowDetails { get; set; }
  public virtual DbSet<Progress> Progresses { get; set; }
  public virtual DbSet<Trigger> Triggers { get; set; }
  public virtual DbSet<TriggerRetry> TriggerRetries { get; set; }

  // Navigation
  public virtual DbSet<Realm> Realms { get; set; }
  public virtual DbSet<Robot> Robots { get; set; }
  public virtual DbSet<RobotCertificate> RobotCertificates { get; set; }
  public virtual DbSet<RobotChassisInfo> RobotChassisInfos { get; set; }
  public virtual DbSet<RobotSystemInfo> RobotSystemInfos { get; set; }
  public virtual DbSet<Waypoint> Waypoints { get; set; }
  public virtual DbSet<WaypointTraffic> WaypointTraffics { get; set; }

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    configurationBuilder
      .Properties<DateTime>()
      .HaveConversion(typeof(UtcValueConverter));
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
    {
      relationship.DeleteBehavior = DeleteBehavior.Restrict;
    }

    // Automation.AutoTasks
    modelBuilder.Entity<AutoTask>()
      .HasMany(e => e.AutoTaskDetails)
      .WithOne(e => e.AutoTask)
      .HasForeignKey(e => e.AutoTaskId)
      .OnDelete(DeleteBehavior.Cascade)
      .IsRequired();
    modelBuilder.Entity<AutoTask>()
      .HasOne(e => e.Realm)
      .WithMany()
      .OnDelete(DeleteBehavior.Cascade)
      .IsRequired();
    modelBuilder.Entity<AutoTask>()
      .HasOne(e => e.AssignedRobot)
      .WithMany(e => e.AssignedTasks)
      .HasForeignKey(e => e.AssignedRobotId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.SetNull);
    modelBuilder.Entity<AutoTask>()
      .HasOne(e => e.Flow)
      .WithMany()
      .HasForeignKey(e => e.FlowId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.SetNull);
    modelBuilder.Entity<AutoTaskDetail>()
      .HasOne(e => e.Waypoint)
      .WithMany()
      .HasForeignKey(e => e.WaypointId)
      .OnDelete(DeleteBehavior.SetNull)
      .IsRequired(false);
    modelBuilder.Entity<AutoTaskJourney>()
      .HasOne(e => e.CurrentProgress)
      .WithMany()
      .HasForeignKey(e => e.CurrentProgressId)
      .OnDelete(DeleteBehavior.SetNull)
      .IsRequired(false);
    // Automation.FlowDetails
    modelBuilder.Entity<Flow>()
      .HasMany(e => e.FlowDetails)
      .WithOne(e => e.Flow)
      .HasForeignKey(e => e.FlowId)
      .OnDelete(DeleteBehavior.Cascade)
      .IsRequired();
    // Automation.TriggerRetries
    modelBuilder.Entity<TriggerRetry>()
      .HasOne(e => e.Trigger)
      .WithMany()
      .HasForeignKey(e => e.TriggerId)
      .IsRequired(true)
      .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<TriggerRetry>()
      .HasOne(e => e.AutoTask)
      .WithMany()
      .HasForeignKey(e => e.AutoTaskId)
      .IsRequired(true)
      .OnDelete(DeleteBehavior.Cascade);
    // Automation.Triggers
    modelBuilder.Entity<Trigger>()
      .HasOne(e => e.ApiKey)
      .WithMany()
      .HasForeignKey(e => e.ApiKeyId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.SetNull);
   
    // Navigation.Robots
    modelBuilder.Entity<Robot>()
      .HasOne(e => e.RobotSystemInfo)
      .WithOne(e => e.Robot)
      .HasForeignKey<RobotSystemInfo>(e => e.RobotId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<Robot>()
      .HasOne(e => e.RobotChassisInfo)
      .WithOne(e => e.Robot)
      .HasForeignKey<RobotChassisInfo>(e => e.RobotId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<Robot>()
      .HasOne(e => e.RobotCertificate)
      .WithOne(e => e.Robot)
      .HasForeignKey<RobotCertificate>(e => e.RobotId)
      .IsRequired()
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Realm>().HasData(
      new Realm
      {
        Id = 1,
        Name = "First Realm",
        Description = "Please update this realm",
        Image = [],
        Resolution = 0,
        OriginX = 0,
        OriginY = 0,
        OriginRotation = 0,
      }
    );
    
    modelBuilder.Entity<Progress>().HasData(
      new Progress
      {
        Id = (int)ProgressState.Template,
        Name = "Template",
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
        Id = (int)ProgressState.PreMoving,
        Name = "PreMoving",
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
        Id = (int)ProgressState.Reserved,
        Name = "Reserved",
        System = true,
        Reserved = true
      }
    );
    base.OnModelCreating(modelBuilder);
  }
}

class UtcValueConverter : ValueConverter<DateTime, DateTime>
{
  public UtcValueConverter() : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) {}
}