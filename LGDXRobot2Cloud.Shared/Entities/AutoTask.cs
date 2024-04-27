using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Shared.Entities
{
  [Table("Navigation.AutoTasks")]
  public class AutoTask
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public string? Name { get; set; }
    
    public ICollection<AutoTaskDetail> Waypoints { get; set; } = [];

    public int Priority { get; set; }

    [ForeignKey("FlowId")]
    public Flow Flow { get; set; } = null!;

    public int FlowId { get; set; }

    [ForeignKey("AssignedRobotId")]
    public Robot? AssignedRobot { get; set; }

    public int? AssignedRobotId { get; set; }

    [ForeignKey("CurrentProgressId")]
    public Progress CurrentProgress { get; set; } =  null!;

    public int CurrentProgressId { get; set; }

    [Precision(6)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Precision(6)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}