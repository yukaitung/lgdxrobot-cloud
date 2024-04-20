using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class FlowDetailBaseDto
  {
    [Required]
    public int Order { get; set; }

    [Required]
    public int ProgressId { get; set; }

    [Required]
    public string ProceedCondition { get; set; } = null!;
    
    public int? StartTriggerId { get; set; }
    public int? EndTriggerId { get; set; }
  }
}