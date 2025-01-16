namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record FlowDetailUpdateBusinessModel
{
  public int? Id { get; set; }

  public required int Order { get; set; }

  public required int ProgressId { get; set; }

  public required int AutoTaskNextControllerId { get; set; }

  public int? TriggerId { get; set; }
}