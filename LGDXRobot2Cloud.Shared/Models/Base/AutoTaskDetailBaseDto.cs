using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class AutoTaskDetailBaseDto
  {
    [Required]
    public int WaypointId { get; set; }

    [Required]
    public int Order { get; set; }
  }
}