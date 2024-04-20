using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Shared.Entities
{
  public class Robot
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(50)]
    public required string Address { get; set; }

    [ForeignKey("DefaultNodesCollectionId")]
    public NodesCollection? DefaultNodesCollection { get; set; }

    public int? DefaultNodesCollectionId { get; set; }

    [ForeignKey("RobotSystemInfoId")]
    public RobotSystemInfo? RobotSystemInfo { get; set; }

    public int? RobotSystemInfoId { get; set; }

    public ICollection<AutoTask> AssignedTasks { get; set; } = [];

    [Precision(3)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Precision(3)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}