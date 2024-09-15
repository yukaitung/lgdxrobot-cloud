using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.Models;

public class Waypoint
{
  public int Id { get; set; }

  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  [Required]
  public double X { get; set; }

  [Required]
  public double Y { get; set; }

  [Required]
  public double Rotation { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }
}
