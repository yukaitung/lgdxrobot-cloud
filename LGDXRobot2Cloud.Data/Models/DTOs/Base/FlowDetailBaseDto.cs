using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Base;

public class FlowDetailBaseDto
{
  [Required]
  public int Order { get; set; }

  [Required]
  public int ProgressId { get; set; }

  [Required]
  public int AutoTaskNextControllerId { get; set; }
  
  public int? StartTriggerId { get; set; }
  public int? EndTriggerId { get; set; }
}
