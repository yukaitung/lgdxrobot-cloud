using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class AutoTaskDetailBlazor
  {
    public int? Id { get; set; }

    public double? CustomX { get; set; }

    public double? CustomY { get; set; }

    public double? CustomRotation { get; set; }

    public WaypointBlazor? Waypoint { get; set; }

    [Required]
    public int? WaypointId { get; set; }

    public string? WaypointName{ get; set; }

    // Will be added before submit
    public int Order { get; set; }
  }
}