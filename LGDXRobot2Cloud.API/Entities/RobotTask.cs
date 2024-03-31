using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Entities
{
  public class RobotTask
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public string? Name { get; set; }
    
    public ICollection<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

    public int Priority { get; set; }

    [ForeignKey("FlowId")]
    public required Flow Flow { get; set; }

    public int FlowId { get; set; }

    [ForeignKey("CurrentProgressId")]
    public required Progress CurrentProgress { get; set; }

    public int CurrentProgressId { get; set; }

    [Precision(6)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Precision(6)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}