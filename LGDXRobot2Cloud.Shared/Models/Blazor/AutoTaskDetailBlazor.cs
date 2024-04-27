using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Shared.Models.Blazor;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class AutoTaskDetailBlazor
  {
    public int? Id { get; set; }

    public WaypointBlazor? Waypoint { get; set; }

    [Required]
    public int? WaypointId { get; set; }

    public string? WaypointName{ get; set; }

    [Required]
    public int Order { get; set; }
  }
}