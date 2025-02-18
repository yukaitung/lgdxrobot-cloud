using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Identity;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Requests;

public record ForgotPasswordRequestDto
{
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public required string Email { get; set; }
}

public static class ForgotPasswordRequestDtoExtensions
{
  public static ForgotPasswordRequestBusinessModel ToBusinessModel (this ForgotPasswordRequestDto forgotPasswordRequestDto)
  {
    return new ForgotPasswordRequestBusinessModel
    {
      Email = forgotPasswordRequestDto.Email
    };
  }
}