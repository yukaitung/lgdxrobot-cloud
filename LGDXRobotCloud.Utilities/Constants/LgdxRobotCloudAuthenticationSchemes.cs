namespace LGDXRobotCloud.Utilities.Constants;

public static class LgdxRobotCloudAuthenticationSchemes
{
  public const string RobotClientsCertificateScheme = "RobotClientsCertificateScheme";
  public const string RobotClientsJwtScheme = "RobotClientsJwtScheme";
  public const string ApiKeyScheme = "ApiKeyScheme";
  public const string CertificationScheme = "CertificationScheme";
  public const string ApiKeyOrCertificationScheme = ApiKeyScheme + "," + CertificationScheme;
}