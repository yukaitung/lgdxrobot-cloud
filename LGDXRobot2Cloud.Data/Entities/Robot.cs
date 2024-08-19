using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Data.Entities;

[Table("Robot.Robots")]
public class Robot
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public Guid Id { get; set; }

  [MaxLength(50)]
  [Required]
  public string Name { get; set; } = null!;

  [MaxLength(100)]
  public string? Address { get; set; }

  [MaxLength(50)]
  public string? Namespace { get; set; }

  public bool IsRealtimeExchange { get; set; }

  public bool IsProtectingHardwareSerialNumber { get; set; }

  [MaxLength(40)]
  public string CertificateThumbprint { get; set; } = null!;

  [MaxLength(40)]
  public string? CertificateThumbprintBackup { get; set; } = null!;

  [Precision(0)]
  public DateTime CertificateNotBefore { get; set; }

  [Precision(0)]
  public DateTime CertificateNotAfter { get; set; }

  [ForeignKey("DefaultNodesCollectionId")]
  public NodesCollection? DefaultNodesCollection { get; set; }

  public int? DefaultNodesCollectionId { get; set; }

  public RobotSystemInfo? RobotSystemInfo { get; set; }

  public RobotChassisInfo? RobotChassisInfo { get; set; }

  public ICollection<AutoTask> AssignedTasks { get; set; } = [];

  [Precision(3)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  
  [Precision(3)]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
