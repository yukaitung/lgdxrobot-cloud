using LGDXRobot2Cloud.Data.Entities;

namespace LGDXRobot2Cloud.Data.Contracts;

public record AutoTaskTriggerContract
{
  public required Trigger Trigger { get; set; }

  public required Dictionary<string, string> Body { get; set; }

  public required int AutoTaskId { get; set; }

  public required string AutoTaskName { get; set; }

  public required Guid RobotId { get; set; }

  public required string RobotName { get; set; }

  public required int RealmId { get; set; }

  public required string RealmName { get; set; }
}