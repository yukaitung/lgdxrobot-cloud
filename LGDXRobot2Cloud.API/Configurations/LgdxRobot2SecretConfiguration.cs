using Microsoft.IdentityModel.Tokens;

namespace LGDXRobot2Cloud.API.Configurations;

public class LgdxRobot2SecretConfiguration
{
  public int RobotClientJwtExpireMins{ get; set; } = 3600;
  public string RobotClientJwtAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256;
  public string RobotClientJwtIssuer { get; set; } = "LGDXRobot2RobotClient";
  public string RobotClientJwtSecret { get; set; } = string.Empty;
}