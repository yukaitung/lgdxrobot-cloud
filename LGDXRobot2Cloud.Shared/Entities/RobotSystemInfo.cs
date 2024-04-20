using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Shared.Entities
{
  public class RobotSystemInfo
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Cpu { get; set; } = null!;

    [MaxLength(100)]
    public string Os { get; set; } = null!;

    public bool Is32Bit { get; set; }

    public bool IsLittleEndian { get; set; }

    [MaxLength(100)]
    public string? Gpu { get; set; }

    public int RamMiB { get; set; }
  }
}