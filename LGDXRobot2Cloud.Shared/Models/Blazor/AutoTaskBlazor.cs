using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Shared.Models.Blazor;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class AutoTaskBlazor
  {
    public int Id { get; set; }
    public string? Name { get; set; }
    public IList<AutoTaskDetailBlazor> TaskDetails { get; set; } = [];
    public int? Priority { get; set; }
    //public RobotListDto? AssignedRobot { get; set; }
    public FlowBlazor Flow { get; set; } = null!;
    public int? FlowId { get; set; }
    public string? FlowName { get; set; }
    public ProgressBlazor CurrentProgress { get; set; } = null!;
    public bool IsTemplate { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}