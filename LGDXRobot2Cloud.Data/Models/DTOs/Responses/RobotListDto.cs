using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class RobotListDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public RobotStatus RobotStatus { get; set; } = RobotStatus.Offline;
  public IEnumerable<double> Batteries { get; set; } = [];
  public bool IsSoftwareEmergencyStop { get; set; }
  public bool IsPauseTaskAssigement { get; set; }
  public bool IsRealtimeExchange { get; set; }
  public string? Address { get; set; }
  public string? Namespace { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
