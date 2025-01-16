namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record FlowUpdateBusinessModel
{
  public required string Name { get; set; }

  public required IList<FlowDetailUpdateBusinessModel> FlowDetails { get; set; } = [];
}