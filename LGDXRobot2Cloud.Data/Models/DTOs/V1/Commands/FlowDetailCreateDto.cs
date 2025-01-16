using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Automation;

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

public static class FlowDetailCreateDtoExtensions
{
  public static FlowDetailCreateBusinessModel ToBusinessModel(this FlowDetailCreateDto flowDetailCreateDto)
  {
    return new FlowDetailCreateBusinessModel {
      Order = flowDetailCreateDto.Order,
      ProgressId = flowDetailCreateDto.ProgressId,
      AutoTaskNextControllerId = flowDetailCreateDto.AutoTaskNextControllerId,
      TriggerId = flowDetailCreateDto.TriggerId,
    };
  }
}