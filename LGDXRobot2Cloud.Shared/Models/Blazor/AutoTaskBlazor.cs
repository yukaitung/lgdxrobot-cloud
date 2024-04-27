using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Shared.Models.Blazor;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class AutoTaskBlazor
  {
    public int Id { get; set; }
    public string? Name { get; set; }
    public IEnumerable<AutoTaskWaypointDetailBlazor> Waypoints { get; set; } = [];
    public int Priority { get; set; }
    //public RobotListDto? AssignedRobot { get; set; }
    public FlowBlazor Flow { get; set; } = null!;
    public ProgressBlazor CurrentProgress { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}