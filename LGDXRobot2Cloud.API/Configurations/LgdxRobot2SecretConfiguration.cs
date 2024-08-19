using Microsoft.IdentityModel.Tokens;

namespace LGDXRobot2Cloud.API.Configurations;

public class LgdxRobot2SecretConfiguration
{
  public int RobotClientsJwtExpireMins{ get; set; } = 3600;
  public string RobotClientsJwtAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256;
  public string RobotClientsJwtIssuer { get; set; } = "LGDXRobot2RobotClients";
  public string RobotClientsJwtSecret { get; set; } = string.Empty;
}