namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record AutoTaskCreateBusinessModel
{
  public string? Name { get; set; }

  public required IEnumerable<AutoTaskDetailCreateBusinessModel> AutoTaskDetails { get; set; } = [];

  public required int Priority { get; set; }

  public required int FlowId { get; set; }

  public required int RealmId { get; set; }

  public Guid? AssignedRobotId { get; set; }

  public bool IsTemplate { get; set; } = false;
}