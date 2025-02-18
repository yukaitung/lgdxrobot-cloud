using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Automation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

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