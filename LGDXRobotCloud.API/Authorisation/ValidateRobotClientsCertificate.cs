using LGDXRobotCloud.Data.DbContexts;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace LGDXRobotCloud.API.Authorisation;

public class ValidateRobotClientsCertificate(LgdxContext context)
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<bool> Validate(X509Certificate2 clientCertificate, Guid robotId)
  {
    var certificate = await _context.RobotCertificates.AsNoTracking()
      .FirstOrDefaultAsync(c => c.RobotId == robotId)
      .Select(c => new {
        c!.Thumbprint,
        c.ThumbprintBackup
      });

    if (certificate == null)
      return false;
    if (certificate.Thumbprint == clientCertificate.Thumbprint)
      return true;
    if (certificate.ThumbprintBackup == clientCertificate.Thumbprint)
      return true;
    return false;
  }
}