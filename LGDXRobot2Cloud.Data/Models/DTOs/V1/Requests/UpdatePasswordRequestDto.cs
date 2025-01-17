using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Identity;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record UpdatePasswordRequestDto
{
  [Required (ErrorMessage = "Please enter a current password.")]
  public required string CurrentPassword { get; set; }

  [Required (ErrorMessage = "Please enter a new password.")]
  public required string NewPassword { get; set; }
}

public static class UpdatePasswordRequestDtoExtensions
{
  public static UpdatePasswordRequestBusinessModel ToBusinessModel(this UpdatePasswordRequestDto updatePasswordRequestDto)
  {
    return new UpdatePasswordRequestBusinessModel 
    {
      CurrentPassword = updatePasswordRequestDto.CurrentPassword,
      NewPassword = updatePasswordRequestDto.NewPassword
    };
  }
}