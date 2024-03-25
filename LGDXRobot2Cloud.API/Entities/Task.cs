using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.API.Entities
{
  public class Task
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public ICollection<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

    [Required]
    public int Priority { get; set; }

    [Required]
    [ForeignKey("FlowId")]
    public Flow Flow { get; set; } = new Flow();

    public int FlowId { get; set; }

    [Required]
    public DateTime CreateAt { get; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}