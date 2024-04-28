using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class RobotBlazor
  {
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Address { get; set; } = null!;
    public bool IsOnline { get; set; }
    public NodesCollectionBlazor? DefaultNodesCollection { get; set; }
    public RobotSystemInfoBlazor? RobotSystemInfo { get; set; }
    public ICollection<AutoTaskBlazor> AssignedTasks { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}