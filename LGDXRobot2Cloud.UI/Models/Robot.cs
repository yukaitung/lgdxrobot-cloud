using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.UI.Models;

public class Robot
{
  public Guid Id { get; set; }

  [Required]
  public string Name { get; set; } = null!;

  public string? Address { get; set; }

  public RobotStatus RobotStatus { get; set; } = RobotStatus.Offline;

  public IEnumerable<double> Batteries { get; set; } = [];

  public bool IsSoftwareEmergencyStop { get; set; }

  public bool IsPauseTaskAssigement { get; set; }

  public string? Namespace { get; set; }

  public bool IsRealtimeExchange { get; set; }

  public bool IsProtectingHardwareSerialNumber { get; set; }

  public RobotCertificate Certificate { get; set; } = null!;

  public NodesCollection? DefaultNodesCollection { get; set; }

  public RobotSystemInfo? RobotSystemInfo { get; set; }

  public RobotChassisInfo? RobotChassisInfo { get; set; }

  public ICollection<AutoTask> AssignedTasks { get; set; } = [];

  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }
}
