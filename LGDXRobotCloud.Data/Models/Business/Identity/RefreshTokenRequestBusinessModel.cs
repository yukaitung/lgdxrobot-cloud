namespace LGDXRobotCloud.Data.Models.Business.Identity;

public record RefreshTokenRequestBusinessModel
{
  public required string RefreshToken { get; set; }
}