using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public abstract class WaypointBaseDto
  {
    [Required]
    [MaxLength(50)]
    public virtual string Name { get; set; } = null!;

    [Required]
    public virtual double X { get; set; }

    [Required]
    public virtual double Y { get; set; }

    [Required]
    public virtual double W { get; set; }
  }
}