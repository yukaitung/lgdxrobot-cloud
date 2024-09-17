using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace LGDXRobot2Cloud.API.Repositories;

public record CertificateDetail 
{
  required public string RootCertificate { get; set; }
  required public string RobotCertificatePrivateKey { get; set; }
  required public string RobotCertificatePublicKey { get; set; }
  required public string RobotCertificateThumbprint { get; set; }
  required public DateTime RobotCertificateNotBefore { get; set; }
  required public DateTime RobotCertificateNotAfter { get; set; }
}

public interface IRobotCertificateRepository
{
  CertificateDetail GenerateRobotCertificate(Guid robotId);
  Task<(IEnumerable<RobotCertificate>, PaginationHelper)> GetRobotCertificatesAsync(int pageNumber, int pageSize);
  Task<RobotCertificate?> GetRobotCertificateAsync(Guid robotCertificateId);
  Task AddRobotCertificateAsync(RobotCertificate robotCertificate);
  void DeleteRobotCertificateAsync(RobotCertificate robotCertificate);
  Task<bool> SaveChangesAsync();
}

public class RobotCertificateRepository(
  LgdxContext context,
  IOptionsSnapshot<LgdxRobot2Configuration> options) : IRobotCertificateRepository
{
  private readonly LgdxContext _context = context;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = options.Value;

  public CertificateDetail GenerateRobotCertificate(Guid robotId)
  {
    X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
    store.Open(OpenFlags.OpenExistingOnly);
    X509Certificate2 rootCertificate = store.Certificates.First(c => c.SerialNumber == _lgdxRobot2Configuration.RootCertificateSN);

    var certificateNotBefore = DateTime.UtcNow;
    var certificateNotAfter = DateTimeOffset.UtcNow.AddDays(_lgdxRobot2Configuration.RobotCertificateValidDay);

    var rsa = RSA.Create();
    var certificateRequest = new CertificateRequest("CN=LGDXRobot2 Robot Certificate for " + robotId.ToString() + ",OID.0.9.2342.19200300.100.1.1=" + robotId.ToString(), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    var certificate = certificateRequest.Create(rootCertificate, certificateNotBefore, certificateNotAfter, RandomNumberGenerator.GetBytes(20));

    return new CertificateDetail {
      RootCertificate = rootCertificate.ExportCertificatePem(),
      RobotCertificatePrivateKey = new string(PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey())),
      RobotCertificatePublicKey = certificate.ExportCertificatePem(),
      RobotCertificateThumbprint = certificate.Thumbprint,
      RobotCertificateNotBefore = certificateNotBefore,
      RobotCertificateNotAfter = certificateNotAfter.DateTime
    };
  }

  public async Task<(IEnumerable<RobotCertificate>, PaginationHelper)> GetRobotCertificatesAsync(int pageNumber, int pageSize)
  {
    var query = _context.RobotCertificates as IQueryable<RobotCertificate>;
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var robotCertificates = await query.AsNoTracking()
      .OrderBy(a => a.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .ToListAsync();
    return (robotCertificates, PaginationHelper);
  }

  public async Task<RobotCertificate?> GetRobotCertificateAsync(Guid robotCertificateId)
  {
    return await _context.RobotCertificates.FirstOrDefaultAsync(a => a.Id == robotCertificateId);
  }

  public async Task AddRobotCertificateAsync(RobotCertificate robotCertificate)
  {
    await _context.RobotCertificates.AddAsync(robotCertificate);
  }

  public void DeleteRobotCertificateAsync(RobotCertificate robotCertificate)
  {
    _context.RobotCertificates.Remove(robotCertificate);
  }

  public async Task<bool> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync() >= 0;
  }
}