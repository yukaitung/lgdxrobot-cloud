using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LGDXRobot2Cloud.Data.DbContexts;

public class LgdxContext(DbContextOptions<LgdxContext> options) : IdentityDbContext<LgdxUser, LgdxRole, string>(options)
{
  // Administration
  public DbSet<ApiKey> ApiKeys { get; set; }

  // Automation
  public DbSet<AutoTask> AutoTasks { get; set; }
  public DbSet<AutoTaskDetail> AutoTasksDetail { get; set; }
  public DbSet<Flow> Flows { get; set; }
  public DbSet<FlowDetail> FlowDetails { get; set; }
  public DbSet<Progress> Progresses { get; set; }
  public DbSet<Trigger> Triggers { get; set; }
  public DbSet<TriggerRetry> TriggerRetries { get; set; }

  // Navigation
  public DbSet<Realm> Realms { get; set; }
  public DbSet<Robot> Robots { get; set; }
  public DbSet<RobotCertificate> RobotCertificates { get; set; }
  public DbSet<RobotChassisInfo> RobotChassisInfos { get; set; }
  public DbSet<RobotSystemInfo> RobotSystemInfos { get; set; }
  public DbSet<Waypoint> Waypoints { get; set; }

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
      .HasMany(e => e.AssignedTasks)
      .WithOne(e => e.AssignedRobot)
      .HasForeignKey(e => e.AssignedRobotId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.SetNull);
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