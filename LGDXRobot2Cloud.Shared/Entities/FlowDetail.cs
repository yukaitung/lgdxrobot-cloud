using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Shared.Entities
{
  [Table("Navigation.FlowDetails")]
  public class FlowDetail
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Order { get; set; }

    [ForeignKey("ProgressId")]
    public Progress Progress { get; set; } = null!;

    public int ProgressId { get; set; }

    [ForeignKey("SystemComponentId")]
    public SystemComponent ProceedCondition { get; set; } = null!;

    public int SystemComponentId { get; set; }
  
    [ForeignKey("StartTriggerId")]
    public Trigger? StartTrigger { get; set; }

    public int? StartTriggerId { get; set; }

    [ForeignKey("EndTriggerId")]
    public Trigger? EndTrigger { get; set; }

    public int? EndTriggerId { get; set; }

    [ForeignKey("FlowId")]
    public Flow Flow { get; set; } = null!;

    public int FlowId { get; set; }
  }
}