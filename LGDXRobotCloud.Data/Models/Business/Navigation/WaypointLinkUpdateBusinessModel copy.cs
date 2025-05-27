namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record WaypointLinkUpdateBusinessModel
{
  public int? Id { get; set; }

  public required int WaypointFromId { get; set; }
  
  public required int WaypointToId { get; set; }
}