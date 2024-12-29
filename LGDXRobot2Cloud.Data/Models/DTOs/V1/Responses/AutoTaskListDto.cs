using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public class AutoTaskListDto
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public int Priority { get; set; }
  public RobotListDto? AssignedRobot { get; set; }
  public FlowListDto Flow { get; set; } = null!;
  public ProgressDto CurrentProgress { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
