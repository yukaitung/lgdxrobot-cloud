namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record AutoTaskUpdateBusinessModel
{
  public string? Name { get; set; }

  public required IEnumerable<AutoTaskDetailUpdateBusinessModel> AutoTaskDetails { get; set; } = [];

  public required int Priority { get; set; }

  public required int FlowId { get; set; }

  public required int RealmId { get; set; }

  public Guid? AssignedRobotId { get; set; }
}