using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class RobotCreateDto
  {
    [Required]
    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Namespace { get; set; }
  }
}