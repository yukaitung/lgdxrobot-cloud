using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class RobotListDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public RobotStatus RobotStatus { get; set; }
  public string? Address { get; set; }
  public int? AssignedTaskId { get; set; }
  public string? AssignedTaskName { get; set; }
  public ProgressState? AssignedTaskProgressState { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
