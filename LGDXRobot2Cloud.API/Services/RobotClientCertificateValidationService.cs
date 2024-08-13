using System.Security.Cryptography.X509Certificates;
using LGDXRobot2Cloud.API.Repositories;

namespace LGDXRobot2Cloud.API.Services;

public class RobotClientCertificateValidationService(IRobotRepository robotRepository)
{
  private readonly IRobotRepository _robotRepository = robotRepository ?? throw new ArgumentNullException(nameof(robotRepository));

  public async Task<bool> ValidateRobotClientCertificate(X509Certificate2 clientCertificate, Guid robotId)
  {
    var robot = await _robotRepository.GetRobotSimpleAsync(robotId);
    if (robot == null)
      return false;
    if (robot.CertificateThumbprint == clientCertificate.Thumbprint)
      return true;
    if (robot.CertificateThumbprintBackup == clientCertificate.Thumbprint)
      return true;
    return false;
  }
}