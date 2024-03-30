using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class FlowDetailEditDto
  {
    public int? Id { get; set; }

    [Required]
    public int Order { get; set; }
    
    [Required]
    public int ProgressId { get; set; }

    [Required]
    public required string ProceedCondition { get; set; }
    public int? StartTriggerId { get; set; }
    public int? EndTriggerId { get; set; }
  }
}