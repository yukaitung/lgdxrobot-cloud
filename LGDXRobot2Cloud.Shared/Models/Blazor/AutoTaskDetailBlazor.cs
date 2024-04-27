using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class AutoTaskDetailBlazor
  {
    public int? Id { get; set; }

    [Required]
    public int? WaypointId { get; set; }

    public string? WaypointName{ get; set; }

    [Required]
    public int Order { get; set; }
  }
}