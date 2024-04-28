namespace LGDXRobot2Cloud.Shared.Models
{
  public class RobotDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public bool IsOnline { get; set; }
    public NodesCollectionListDto? DefaultNodesCollection { get; set; }
    public RobotSystemInfoDto? RobotSystemInfo { get; set; }
    public ICollection<AutoTaskListDto> AssignedTasks { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}