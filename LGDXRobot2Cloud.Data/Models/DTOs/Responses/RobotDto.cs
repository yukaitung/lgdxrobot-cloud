using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class RobotDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string Address { get; set; } = null!;
  public bool IsRealtimeExchange { get; set; }
  public bool IsProtectingHardwareSerialNumber { get; set; }
  public RobotCertificateDto Certificate { get; set; } = null!;
  public NodesCollectionListDto? DefaultNodesCollection { get; set; }
  public RobotSystemInfoDto? RobotSystemInfo { get; set; }
  public RobotChassisInfoDto? RobotChassisInfo { get; set; }
  public ICollection<AutoTaskListDto> AssignedTasks { get; set; } = [];
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
