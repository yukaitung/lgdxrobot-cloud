using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.API.Utilities;

namespace LGDXRobot2Cloud.API.Models
{
  public class AutoTaskCreateDto
  {
    public string? Name { get; set; }
    public IEnumerable<AutoTaskWaypointDetailCreateDto> Waypoints { get; set; } = new List<AutoTaskWaypointDetailCreateDto>();
    public int Priority { get; set; }

    [Required]
    public required int FlowId { get; set; }
    public bool Saved { get; set; } = false;
  }
}