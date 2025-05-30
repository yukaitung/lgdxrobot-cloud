using Microsoft.IdentityModel.Tokens;

namespace LGDXRobotCloud.API.Configurations;

public class LgdxRobotCloudSecretConfiguration
{
  public int LgdxUserAccessTokenExpiresMins{ get; set; } = 30;
  public int LgdxUserRefreshTokenExpiresMins{ get; set; } = 1440; // 1 day
  public string LgdxUserJwtAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256;
  public string LgdxUserJwtIssuer { get; set; } = "LGDXRobotCloudUsers";
  public string LgdxUserJwtSecret { get; set; } = string.Empty;
  public int RobotClientsJwtExpireMins{ get; set; } = 3600;
  public string RobotClientsJwtAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256;
  public string RobotClientsJwtIssuer { get; set; } = "LGDXRobotCloudRobotClients";
  public string RobotClientsJwtSecret { get; set; } = string.Empty;
}