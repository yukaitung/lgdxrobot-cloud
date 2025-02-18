namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record AutoTaskDetailUpdateBusinessModel
{
  public int? Id { get; set; }

  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  
  public int? WaypointId { get; set; }

  public required int Order { get; set; }
}