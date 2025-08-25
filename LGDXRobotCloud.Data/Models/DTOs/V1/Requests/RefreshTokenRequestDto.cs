using LGDXRobotCloud.Data.Models.Business.Identity;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Requests;

public record RefreshTokenRequestDto
{
  public required string RefreshToken { get; set; }
}

public static class RefreshTokenRequestDtoExtensions
{
  public static RefreshTokenRequestBusinessModel ToBusinessModel(this RefreshTokenRequestDto refreshTokenRequestDto)
  {
    return new()
    {
      RefreshToken = refreshTokenRequestDto.RefreshToken
    };
  }
}