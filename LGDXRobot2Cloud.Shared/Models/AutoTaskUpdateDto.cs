using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class AutoTaskUpdateDto
  {
    public string? Name { get; set; }
    public IEnumerable<AutoTaskWaypointDetailUpdateDto> Waypoints { get; set; } = new List<AutoTaskWaypointDetailUpdateDto>();
    public int Priority { get; set; }
   
   [Required]
   public required int FlowId { get; set; }
  }
}