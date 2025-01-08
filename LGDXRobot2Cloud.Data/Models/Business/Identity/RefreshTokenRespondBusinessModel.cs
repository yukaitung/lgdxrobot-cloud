using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Identity;

public record RefreshTokenResponseBusinessModel
{
  public required string AccessToken { get; set; }

  public required string RefreshToken { get; set; }

  public required int ExpiresMins { get; set; }
}

public static class RefreshTokenResponseBusinessModelExtensions
{
  public static RefreshTokenResponseDto ToDto(this RefreshTokenResponseBusinessModel refreshTokenResponseBusinessModel)
  {
    return new()
    {
      AccessToken = refreshTokenResponseBusinessModel.AccessToken,
      RefreshToken = refreshTokenResponseBusinessModel.RefreshToken,
      ExpiresMins = refreshTokenResponseBusinessModel.ExpiresMins
    };
  }
}