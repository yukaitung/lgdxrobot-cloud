using LGDXRobot2Cloud.Data.Entities;

namespace LGDXRobot2Cloud.Data.Contracts;

public record AutoTaskTriggerContract
{
  public required Trigger Trigger { get; set; }

  public required Dictionary<string, string> Body { get; set; }
}