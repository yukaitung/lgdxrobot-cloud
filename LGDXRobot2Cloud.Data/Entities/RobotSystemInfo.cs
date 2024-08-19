using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Data.Entities;

[Table("Robot.RobotSystemInfos")]
public class RobotSystemInfo
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(100)]
  public string Motherboard { get; set; } = null!;

  [MaxLength(100)]
  public string MotherboardSerialNumber { get; set; } = null!;

  [MaxLength(100)]
  public string Cpu { get; set; } = null!;

  public bool IsLittleEndian { get; set; }

  public int RamMiB { get; set; }

  [MaxLength(100)]
  public string? Gpu { get; set; }

  [MaxLength(100)]
  public string Os { get; set; } = null!;

  public bool Is32Bit { get; set; }

  [ForeignKey("RobotId")]
  public Robot Robot { get; set; } = null!;

  public Guid RobotId { get; set; }

  [Precision(3)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  
  [Precision(3)]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
