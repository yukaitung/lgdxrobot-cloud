using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.API.Entities
{
  public class FlowDetail
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Order { get; set; }

    [ForeignKey("ProgressId")]
    public required Progress Progress { get; set; }

    public int ProgressId { get; set; }

    [ForeignKey("SystemComponentId")]
    public required SystemComponent ProceedCondition { get; set; }

    public int SystemComponentId { get; set; }
  
    [ForeignKey("StartTriggerId")]
    public Trigger? StartTrigger { get; set; }

    public int? StartTriggerId { get; set; }

    [ForeignKey("EndTriggerId")]
    public Trigger? EndTrigger { get; set; }

    public int? EndTriggerId { get; set; }

    [ForeignKey("FlowId")]
    public required Flow Flow { get; set; }

    public int FlowId { get; set; }
  }
}