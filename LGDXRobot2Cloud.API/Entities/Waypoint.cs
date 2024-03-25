using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.API.Entities
{
  public class Waypoint
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string? Name { get; set; } = string.Empty;
    
    [Required]
    public double X { get; set; }

    [Required]
    public double Y { get; set; }

    [Required]
    public double W { get; set; }

    [Required]
    public DateTime CreateAt { get; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}