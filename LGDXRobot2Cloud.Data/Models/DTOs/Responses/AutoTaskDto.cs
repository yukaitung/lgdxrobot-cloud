namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class AutoTaskDto
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public IEnumerable<AutoTaskDetailDto> Details { get; set; } = [];
  public int Priority { get; set; }
  public RobotListDto? AssignedRobot { get; set; }
  public FlowListDto Flow { get; set; } = null!;
  public ProgressDto CurrentProgress { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
