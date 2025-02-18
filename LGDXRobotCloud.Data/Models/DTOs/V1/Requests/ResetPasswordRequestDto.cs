using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Identity;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Requests;

public record ResetPasswordRequestDto
{
  [Required (ErrorMessage = "Please enter a username.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public required string Email { get; set; }

  [Required (ErrorMessage = "Please enter a token.")]
  public required string Token { get; set; }

  [Required (ErrorMessage = "Please enter a new password.")]
  public required string NewPassword { get; set; }
}

public static class ResetPasswordRequestDtoExtensions
{
  public static ResetPasswordRequestBusinessModel ToBusinessModel (this ResetPasswordRequestDto resetPasswordRequestDto)
  {
    return new ResetPasswordRequestBusinessModel
    {
      Email = resetPasswordRequestDto.Email,
      Token = resetPasswordRequestDto.Token,
      NewPassword = resetPasswordRequestDto.NewPassword
    };
  }
}