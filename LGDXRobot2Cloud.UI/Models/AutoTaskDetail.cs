using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.Models;
public class AutoTaskDetail
{
  public int? Id { get; set; }

  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }

  public Waypoint? Waypoint { get; set; }

  [Required]
  public int? WaypointId { get; set; }

  public string? WaypointName{ get; set; }

  // Will be added before submit
  public int Order { get; set; }
}
