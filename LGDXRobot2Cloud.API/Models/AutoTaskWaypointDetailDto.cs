namespace LGDXRobot2Cloud.API.Models
{
  public class AutoTaskWaypointDetailDto
  {
    public int? Id { get; set; }
    public int Order { get; set; }
    public required WaypointDto Waypoint { get; set; }
  }
}