using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.Entities;

[Table("Automation.AutoTaskJourney")]
public class AutoTaskJourney
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  // Prevent error when the Progress being deleted
  [ForeignKey("CurrentProgressId")]
  public Progress? CurrentProgress { get; set; } = null!;

  public int? CurrentProgressId { get; set; }

  [ForeignKey("AutoTaskId")]
  public AutoTask AutoTask { get; set; } = null!;

  public int AutoTaskId { get; set; }

  [Precision(0)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
