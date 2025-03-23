using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Identity;

public record LoginResponseBusinessModel
{
  public required string AccessToken { get; set; }

  public required string RefreshToken { get; set; }

  public required int ExpiresMins { get; set; }

  public required bool RequiresTwoFactor { get; set; }
}

public static class LoginResponseBusinessModelExtensions
{
  public static LoginResponseDto ToDto(this LoginResponseBusinessModel loginResponseBusinessModel)
  {
    return new LoginResponseDto {
      AccessToken = loginResponseBusinessModel.AccessToken,
      RefreshToken = loginResponseBusinessModel.RefreshToken,
      ExpiresMins = loginResponseBusinessModel.ExpiresMins,
      RequiresTwoFactor = loginResponseBusinessModel.RequiresTwoFactor
    };
  }
}