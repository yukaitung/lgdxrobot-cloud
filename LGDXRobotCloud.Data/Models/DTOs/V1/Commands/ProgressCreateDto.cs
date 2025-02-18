using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Automation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record ProgressCreateDto
{
  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public required string Name { get; set; }
}

public static class ProgressCreateDtoExtensions
{
  public static ProgressCreateBusinessModel ToBusinessModel(this ProgressCreateDto progressCreateDto)
  {
    return new ProgressCreateBusinessModel {
      Name = progressCreateDto.Name,
    };
  }
}