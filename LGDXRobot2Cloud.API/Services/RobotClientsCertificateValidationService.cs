using System.Security.Cryptography.X509Certificates;
using LGDXRobot2Cloud.API.Repositories;

namespace LGDXRobot2Cloud.API.Services;

public class RobotClientsCertificateValidationService(IRobotCertificateRepository robotCertificateRepository)
{
  private readonly IRobotCertificateRepository _robotCertificateRepository = robotCertificateRepository;

  public async Task<bool> ValidateRobotClientsCertificate(X509Certificate2 clientCertificate, Guid robotId)
  {
    var certificate = await _robotCertificateRepository.GetRobotCertificateFromRobotIdAsync(robotId);
    if (certificate == null)
      return false;
    if (certificate.Thumbprint == clientCertificate.Thumbprint)
      return true;
    if (certificate.ThumbprintBackup == clientCertificate.Thumbprint)
      return true;
    return false;
  }
}