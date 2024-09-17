using System.Security.Cryptography.X509Certificates;
using LGDXRobot2Cloud.API.Repositories;

namespace LGDXRobot2Cloud.API.Services;

public class RobotClientsCertificateValidationService(IRobotRepository robotRepository)
{
  private readonly IRobotRepository _robotRepository = robotRepository ?? throw new ArgumentNullException(nameof(robotRepository));

  public async Task<bool> ValidateRobotClientsCertificate(X509Certificate2 clientCertificate, Guid robotId)
  {
    var robot = await _robotRepository.GetRobotSimpleAsync(robotId);
    if (robot == null)
      return false;
    if (robot.Certificate.Thumbprint == clientCertificate.Thumbprint)
      return true;
    if (robot.Certificate.ThumbprintBackup == clientCertificate.Thumbprint)
      return true;
    return false;
  }
}