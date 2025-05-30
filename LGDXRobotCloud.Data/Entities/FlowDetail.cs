using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.Entities;

[Table("Automation.FlowDetails")]
[Index(nameof(Order))]
public class FlowDetail
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  public int Order { get; set; }

  [ForeignKey("ProgressId")]
  public Progress Progress { get; set; } = null!;

  public int ProgressId { get; set; }

  public int AutoTaskNextControllerId { get; set; }

  [ForeignKey("TriggerId")]
  public Trigger? Trigger { get; set; }

  public int? TriggerId { get; set; }

  [ForeignKey("FlowId")]
  public Flow Flow { get; set; } = null!;

  public int FlowId { get; set; }
}
