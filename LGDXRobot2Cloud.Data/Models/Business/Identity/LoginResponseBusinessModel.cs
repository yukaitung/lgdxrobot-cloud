using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Identity;

public record LoginResponseBusinessModel
{
  public required string AccessToken { get; set; }

  public required string RefreshToken { get; set; }

  public required int ExpiresMins { get; set; }
}

public static class LoginResponseBusinessModelExtensions
{
  public static LoginResponseDto ToDto(this LoginResponseBusinessModel loginResponseBusinessModel)
  {
    return new LoginResponseDto {
      AccessToken = loginResponseBusinessModel.AccessToken,
      RefreshToken = loginResponseBusinessModel.RefreshToken,
      ExpiresMins = loginResponseBusinessModel.ExpiresMins
    };
  }
}