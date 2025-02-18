namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public sealed record RefreshTokenResponseDto
{
  public required string AccessToken { get; set; }

  public required string RefreshToken { get; set; }

  public required int ExpiresMins { get; set; }
}