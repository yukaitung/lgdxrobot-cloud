using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Automation;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record FlowDetailUpdateDto
{
  public int? Id { get; set; }

  [Required]
  public required int Order { get; set; }

  [Required (ErrorMessage = "Please select a progress.")]
  public required int ProgressId { get; set; }

  [Required]
  public required int AutoTaskNextControllerId { get; set; }
  
  public int? TriggerId { get; set; }
}

public static class FlowDetailUpdateDtoExtensions
{
  public static FlowDetailUpdateBusinessModel ToBusinessModel(this FlowDetailUpdateDto flowDetailUpdateDto)
  {
    return new FlowDetailUpdateBusinessModel {
      Id = flowDetailUpdateDto.Id,
      Order = flowDetailUpdateDto.Order,
      ProgressId = flowDetailUpdateDto.ProgressId,
      AutoTaskNextControllerId = flowDetailUpdateDto.AutoTaskNextControllerId,
      TriggerId = flowDetailUpdateDto.TriggerId,
    };
  }
}