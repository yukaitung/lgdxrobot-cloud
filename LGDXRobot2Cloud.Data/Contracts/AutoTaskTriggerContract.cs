using LGDXRobot2Cloud.Data.Entities;

namespace LGDXRobot2Cloud.Data.Contracts;

public record AutoTaskTriggerContract
{
  public AutoTask AutoTask { get; init; } = null!;
  public FlowDetail FlowDetail { get; init; } = null!;
}