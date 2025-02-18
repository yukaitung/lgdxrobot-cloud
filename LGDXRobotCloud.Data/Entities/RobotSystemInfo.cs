using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobotCloud.Data.Entities;

[Table("Navigation.RobotSystemInfos")]
public class RobotSystemInfo
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(100)]
  public string Cpu { get; set; } = null!;

  public bool IsLittleEndian { get; set; }

  [MaxLength(100)]
  public string Motherboard { get; set; } = null!;

  [MaxLength(100)]
  public string MotherboardSerialNumber { get; set; } = null!;

  public int RamMiB { get; set; }

  [MaxLength(100)]
  public string? Gpu { get; set; }

  [MaxLength(100)]
  public string Os { get; set; } = null!;

  public bool Is32Bit { get; set; }

  [MaxLength(100)]
  public string? McuSerialNumber { get; set; } = null!;

  [ForeignKey("RobotId")]
  public Robot Robot { get; set; } = null!;

  public Guid RobotId { get; set; }
}
