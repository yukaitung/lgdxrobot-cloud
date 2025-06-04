namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record WaypointTrafficUpdateBusinessModel
{
  public required int WaypointFromId { get; set; }
  
  public required int WaypointToId { get; set; }
}