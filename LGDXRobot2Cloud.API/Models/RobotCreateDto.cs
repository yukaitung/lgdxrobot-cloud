using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class RobotCreateDto
  {
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Address { get; set; }
  }
}