namespace LGDXRobot2Cloud.Data.Models.Business.Identity;

public record RefreshTokenResponseBusinessModel
{
  public required string AccessToken { get; set; }

  public required string RefreshToken { get; set; }

  public required int ExpiresMins { get; set; }
}