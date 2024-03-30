using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.API.Entities
{
  public class FlowDetail
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey("ProgressId")]
    public Progress Progress { get; set; } = null!;

    public int ProgressId { get; set; }

    [Required]
    [ForeignKey("SystemComponentId")]
    public SystemComponent ProceedCondition { get; set; } = null!;

    public int SystemComponentId { get; set; }
  
    [ForeignKey("StartTriggerId")]
    public Trigger? StartTrigger { get; set; }

    public int StartTriggerId { get; set; }

    [ForeignKey("EndTriggerId")]
    public Trigger? EndTrigger { get; set; }

    public int EndTriggerId { get; set; }

    [ForeignKey("FlowId")]
    public Flow Flow { get; set; } = null!;

    public int FlowId { get; set; }
  }
}