using LGDXRobot2Cloud.Data.Models.Business.Identity;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public sealed record RefreshTokenRequestDto
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