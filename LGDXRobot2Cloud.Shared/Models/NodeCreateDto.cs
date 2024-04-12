using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodeCreateDto
  {
    [MaxLength(50)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    [Required]
    public string ProcessName { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Arguments { get; set; }
  }
}