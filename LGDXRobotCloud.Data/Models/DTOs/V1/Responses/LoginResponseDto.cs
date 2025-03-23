namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record LoginResponseDto
{
  public required string AccessToken { get; set; }

  public required string RefreshToken { get; set; }

  public required int ExpiresMins { get; set; }

  public required bool RequiresTwoFactor { get; set; }
}