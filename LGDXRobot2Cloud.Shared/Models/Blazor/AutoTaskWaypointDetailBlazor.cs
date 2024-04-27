using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class AutoTaskWaypointDetailBlazor
  {
    public int? Id { get; set; }

    [Required]
    public int WaypointId { get; set; }

    [Required]
    public int Order { get; set; }
  }
}