using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class AutoTaskWaypointDetailCreateDto
  {
    [Required]
    public required int WaypointId { get; set; }

    [Required]
    public required int Order { get; set; }
  }
}