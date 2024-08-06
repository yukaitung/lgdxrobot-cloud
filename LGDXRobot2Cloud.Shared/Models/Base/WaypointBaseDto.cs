using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class WaypointBaseDto
  {
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Required]
    public double X { get; set; }

    [Required]
    public double Y { get; set; }

    [Required]
    public double Rotation { get; set; }
  }
}