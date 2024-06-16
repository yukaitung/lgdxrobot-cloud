using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class AutoTaskBlazor
  {
    public int Id { get; set; }

    [MaxLength(50)]
    public string? Name { get; set; }
    public IList<AutoTaskDetailBlazor> Details { get; set; } = [];
    public int? Priority { get; set; }
    public RobotBlazor? AssignedRobot { get; set; }
    public Guid? AssignedRobotId { get; set; }
    public string? AssignedRobotName { get; set; }
    public FlowBlazor Flow { get; set; } = null!;
    
    [Required]
    public int? FlowId { get; set; }
    public string? FlowName { get; set; }
    public ProgressBlazor CurrentProgress { get; set; } = null!;
    public bool IsTemplate { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}