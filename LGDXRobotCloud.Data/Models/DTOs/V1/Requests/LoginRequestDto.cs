using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Identity;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Requests;

public record LoginRequestDto
{
  [Required (ErrorMessage = "Please enter a username.")]
  public required string Username { get; set; }

  [Required (ErrorMessage = "Please enter a password.")]
  public required string Password { get; set; }

  public string? TwoFactorCode { get; set; }
  
  public string? TwoFactorRecoveryCode { get; set; }
}

public static class LoginRequestDtoExtensions
{
  public static LoginRequestBusinessModel ToBusinessModel (this LoginRequestDto loginRequestDto)
  {
    return new LoginRequestBusinessModel
    {
      Username = loginRequestDto.Username,
      Password = loginRequestDto.Password,
      TwoFactorCode = loginRequestDto.TwoFactorCode,
      TwoFactorRecoveryCode = loginRequestDto.TwoFactorRecoveryCode
    };
  }
}