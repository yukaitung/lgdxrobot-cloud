using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LGDXRobot2Cloud.Data.DbContexts;

public class LgdxContext(DbContextOptions<LgdxContext> options) : IdentityDbContext<LgdxUser, LgdxRole, string>(options)
{
  // Navigation
  public DbSet<AutoTask> AutoTasks { get; set; }
  public DbSet<AutoTaskDetail> AutoTasksDetail { get; set; }
  public DbSet<Flow> Flows { get; set; }
  public DbSet<FlowDetail> FlowDetails { get; set; }
  public DbSet<Progress> Progresses { get; set; }
  public DbSet<Trigger> Triggers { get; set; }
  public DbSet<Waypoint> Waypoints { get; set; }
  public DbSet<Realm> Realms { get; set; }

  // Robot
  public DbSet<Robot> Robots { get; set; }
  public DbSet<RobotCertificate> RobotCertificates { get; set; }
  public DbSet<RobotSystemInfo> RobotSystemInfos { get; set; }
  public DbSet<RobotChassisInfo> RobotChassisInfos { get; set; }

  // Setting
  public DbSet<ApiKey> ApiKeys { get; set; }

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
      .IsRequired(false)
      .OnDelete(DeleteBehavior.SetNull);
    // One Robot has one RobotSystemInfo
    modelBuilder.Entity<Robot>()
      .HasOne(e => e.RobotSystemInfo)
      .WithOne(e => e.Robot)
      .HasForeignKey<RobotSystemInfo>(e => e.RobotId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.Cascade);
    // One Robot has one RobotChassisInfo
    modelBuilder.Entity<Robot>()
      .HasOne(e => e.RobotChassisInfo)
      .WithOne(e => e.Robot)
      .HasForeignKey<RobotChassisInfo>(e => e.RobotId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.Cascade);
    // One Robot has one RobotCertificate
    modelBuilder.Entity<Robot>()
      .HasOne(e => e.RobotCertificate)
      .WithOne(e => e.Robot)
      .HasForeignKey<RobotCertificate>(e => e.RobotId)
      .IsRequired()
      .OnDelete(DeleteBehavior.Cascade);
    // One Flow has many FlowDetails
    modelBuilder.Entity<Flow>()
      .HasMany(e => e.FlowDetails)
      .WithOne(e => e.Flow)
      .HasForeignKey(e => e.FlowId)
      .IsRequired();
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