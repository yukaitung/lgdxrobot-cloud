namespace LGDXRobot2Cloud.API.Configurations;

public class LgdxRobot2Configuration
{
  public string RootCertificateSN { get; set; } = string.Empty;
  public int RobotCertificateValidDay { get; set; } = 365;
  public int ApiMaxPageSize { get; set; } = 100;
}
