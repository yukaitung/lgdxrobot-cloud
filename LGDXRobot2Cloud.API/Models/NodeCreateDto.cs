using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class NodeCreateDto
  {
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public required string ProcessName { get; set; }

    [MaxLength(200)]
    public string? Arguments { get; set; }
  }
}