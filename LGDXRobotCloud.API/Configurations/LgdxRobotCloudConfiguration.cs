namespace LGDXRobotCloud.API.Configurations;

public class LgdxRobotCloudConfiguration
{
  public string RootCertificateSN { get; set; } = string.Empty;
  public int RobotCertificateValidDay { get; set; } = 365;
  public int ApiMaxPageSize { get; set; } = 100;
}
