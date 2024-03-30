using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class WaypointCreateDto
  {
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Required]
    public double X { get; set; }

    [Required]
    public double Y { get; set; }

    [Required]
    public double W { get; set; }
  }
}