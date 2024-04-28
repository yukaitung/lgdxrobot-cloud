using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Shared.Entities
{
  [Table("Robot.RobotSystemInfos")]
  public class RobotSystemInfo
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Cpu { get; set; } = null!;

    public bool IsLittleEndian { get; set; }

    public int RamMiB { get; set; }

    [MaxLength(100)]
    public string? Gpu { get; set; }

    [MaxLength(100)]
    public string Os { get; set; } = null!;

    public bool Is32Bit { get; set; }
  }
}