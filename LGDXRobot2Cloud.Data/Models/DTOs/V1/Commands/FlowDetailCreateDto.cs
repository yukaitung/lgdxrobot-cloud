using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record FlowDetailCreateDto
{
  [Required]
  public required int Order { get; set; }

  [Required (ErrorMessage = "Please select a progress.")]
  public required int ProgressId { get; set; }

  [Required]
  public required int AutoTaskNextControllerId { get; set; }
  
  public int? TriggerId { get; set; }
}
