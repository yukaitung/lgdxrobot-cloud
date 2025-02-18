using LGDXRobotCloud.Data.Entities;

namespace LGDXRobotCloud.Data.Contracts;

public record AutoTaskTriggerContract
{
  public required Trigger Trigger { get; set; }

  public required Dictionary<string, string> Body { get; set; }

  public required int AutoTaskId { get; set; }

  public string AutoTaskName { get; set; } = string.Empty;

  public required Guid RobotId { get; set; }

  public required string RobotName { get; set; }

  public required int RealmId { get; set; }

  public required string RealmName { get; set; }
}