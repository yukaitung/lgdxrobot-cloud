using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class NodeBaseDto
  {
    [MaxLength(50)]
    [Required]
    public string Name { get; set; } = null!;

    [MaxLength(50)]
    [Required]
    public string ProcessName { get; set; } = null!;
    
    [MaxLength(200)]
    public string? Arguments { get; set; }
  }
}