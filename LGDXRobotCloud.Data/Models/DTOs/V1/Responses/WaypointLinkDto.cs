namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record WaypointLinkDto
{
  public int Id { get; set; }

  public int WaypointFromId { get; set; }

  public int WaypointToId { get; set; }
}