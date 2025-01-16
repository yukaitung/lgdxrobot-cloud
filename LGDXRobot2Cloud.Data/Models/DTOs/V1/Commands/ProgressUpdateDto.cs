using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Automation;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record ProgressUpdateDto
{
  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public required string Name { get; set; }
}

public static class ProgressUpdateDtoExtensions
{
  public static ProgressUpdateBusinessModel ToBusinessModel(this ProgressUpdateDto progressUpdateDto)
  {
    return new ProgressUpdateBusinessModel {
      Name = progressUpdateDto.Name,
    };
  }
}