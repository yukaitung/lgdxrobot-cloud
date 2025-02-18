using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.Entities;

[Table("Automation.TriggerRetries")]
public class TriggerRetry
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [ForeignKey("TriggerId")]
  public Trigger Trigger { get; set; } = null!;

  public int TriggerId { get; set; }

  [ForeignKey("AutoTaskId")]
  public AutoTask AutoTask { get; set; } = null!;

  public int AutoTaskId { get; set; }

  [Column(TypeName = "TEXT")]
  public string Body { get; set; } = null!;

  [Precision(3)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}