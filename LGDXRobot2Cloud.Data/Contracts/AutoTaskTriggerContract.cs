using LGDXRobot2Cloud.Data.Entities;

namespace LGDXRobot2Cloud.Data.Contracts;

public record AutoTaskTriggerContract
{
  public required Trigger Trigger { get; set; }

  // Decision
  public required int AutoTaskNextControllerId { get; set; }
  public string? NextToken { get; set; }

  // Data
  public required int AutoTaskId { get; set; }
  public string AutoTaskName { get; set; } = string.Empty;
  public required int AutoTaskCurrentProgressId { get; set; }
  public required string AutoTaskCurrentProgressName { get; set; }
  public required Guid RobotId { get; set; }
  public required string RobotName { get; set; }
  public required int RealmId { get; set; }
  public required string RealmName { get; set; }
}