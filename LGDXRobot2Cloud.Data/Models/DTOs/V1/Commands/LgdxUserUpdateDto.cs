using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Identity;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record LgdxUserUpdateDto
{
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  [Required (ErrorMessage = "Please enter an email.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public required string Email { get; set; }
}

public static class LgdxUserUpdateDtoExtensions
{
  public static LgdxUserUpdateBusinessModel ToBusinessModel(this LgdxUserUpdateDto model)
  {
    return new LgdxUserUpdateBusinessModel {
      Name = model.Name,
      Email = model.Email,
    };
  }
}