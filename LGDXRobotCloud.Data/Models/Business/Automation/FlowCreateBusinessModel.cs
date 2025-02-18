namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record FlowCreateBusinessModel
{
  public required string Name { get; set; }

  public required IList<FlowDetailCreateBusinessModel> FlowDetails { get; set; } = [];
}