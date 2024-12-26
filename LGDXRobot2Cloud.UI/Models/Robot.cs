using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.Models;

public class Robot
{
  public Guid Id { get; set; }

  [Required]
  public string Name { get; set; } = null!;

  public string? Address { get; set; }

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
