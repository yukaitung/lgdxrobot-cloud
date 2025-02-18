using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LGDXRobotCloud.API.Services.Administration;

public interface IRobotCertificateService
{
  Task<(IEnumerable<RobotCertificateListBusinessModel>, PaginationHelper)> GetRobotCertificatesAsync(int pageNumber, int pageSize);
  Task<RobotCertificateBusinessModel> GetRobotCertificateAsync(Guid robotCertificateId);
  RobotCertificateIssueBusinessModel IssueRobotCertificate(Guid robotId);
  Task<RobotCertificateRenewBusinessModel> RenewRobotCertificateAsync(RobotCertificateRenewRequestBusinessModel robotCertificateRenewRequestBusinessModel);

  RootCertificateBusinessModel? GetRootCertificate();
}

public class RobotCertificateService(
    LgdxContext context,
    IOptionsSnapshot<LgdxRobotCloudConfiguration> options
  ) : IRobotCertificateService
{
  private readonly LgdxContext _context = context;
  private readonly LgdxRobotCloudConfiguration _lgdxRobotCloudConfiguration = options.Value;

  private record CertificateDetail 
  {
    required public string RootCertificate { get; set; }
    required public string RobotCertificatePrivateKey { get; set; }
    required public string RobotCertificatePublicKey { get; set; }
    required public string RobotCertificateThumbprint { get; set; }
    required public DateTime RobotCertificateNotBefore { get; set; }
    required public DateTime RobotCertificateNotAfter { get; set; }
  }
  
  public async Task<(IEnumerable<RobotCertificateListBusinessModel>, PaginationHelper)> GetRobotCertificatesAsync(int pageNumber, int pageSize)
  {
    var query = _context.RobotCertificates as IQueryable<RobotCertificate>;
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var robotCertificates = await query.AsNoTracking()
      .OrderBy(a => a.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(a => new RobotCertificateListBusinessModel {
        Id = a.Id,
        Thumbprint = a.Thumbprint,
        ThumbprintBackup = a.ThumbprintBackup,
        NotBefore = a.NotBefore,
        NotAfter = a.NotAfter
      })
      .ToListAsync();
    return (robotCertificates, PaginationHelper);
  }

  public async Task<RobotCertificateBusinessModel> GetRobotCertificateAsync(Guid robotCertificateId)
  {
    return await _context.RobotCertificates.AsNoTracking()
      .Where(a => a.Id == robotCertificateId)
      .Include(a => a.Robot)
      .Select(a => new RobotCertificateBusinessModel {
        Id = a.Id,
        RobotId = a.Robot.Id,
        RobotName = a.Robot.Name,
        Thumbprint = a.Thumbprint,
        ThumbprintBackup = a.ThumbprintBackup,
        NotBefore = a.NotBefore,
        NotAfter = a.NotAfter
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  private CertificateDetail GenerateRobotCertificate(Guid robotId)
  {
    X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
    store.Open(OpenFlags.OpenExistingOnly);
    X509Certificate2 rootCertificate = store.Certificates.First(c => c.SerialNumber == _lgdxRobotCloudConfiguration.RootCertificateSN);

    var certificateNotBefore = DateTime.UtcNow;
    var certificateNotAfter = DateTimeOffset.UtcNow.AddDays(_lgdxRobotCloudConfiguration.RobotCertificateValidDay);

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

  public RobotCertificateIssueBusinessModel IssueRobotCertificate(Guid robotId)
  {
    var certificate = GenerateRobotCertificate(robotId);
    return new RobotCertificateIssueBusinessModel {
      RootCertificate = certificate.RootCertificate,
      RobotCertificatePrivateKey = certificate.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = certificate.RobotCertificatePublicKey,
      RobotCertificateThumbprint = certificate.RobotCertificateThumbprint,
      RobotCertificateNotBefore = certificate.RobotCertificateNotBefore,
      RobotCertificateNotAfter = certificate.RobotCertificateNotAfter
    };
  }

  public async Task<RobotCertificateRenewBusinessModel> RenewRobotCertificateAsync(RobotCertificateRenewRequestBusinessModel robotCertificateRenewRequestBusinessModel)
  {
    var certificate = _context.RobotCertificates
      .Where(c => c.Id == robotCertificateRenewRequestBusinessModel.CertificateId)
      .FirstOrDefault() 
        ?? throw new LgdxNotFound404Exception();

      var newCertificate = GenerateRobotCertificate(certificate.RobotId);
      if (robotCertificateRenewRequestBusinessModel.RevokeOldCertificate)
      {
        certificate.ThumbprintBackup = null;
      }
      else
      {
        certificate.ThumbprintBackup = certificate.Thumbprint;
      }
      certificate.Thumbprint = newCertificate.RobotCertificateThumbprint;
      certificate.NotBefore = DateTime.SpecifyKind(newCertificate.RobotCertificateNotBefore, DateTimeKind.Utc);
      certificate.NotAfter = DateTime.SpecifyKind(newCertificate.RobotCertificateNotAfter, DateTimeKind.Utc);
      await _context.SaveChangesAsync();

      var robot = await _context.Robots.AsNoTracking()
        .Where(r => r.Id == certificate.RobotId)
        .Select(r => new {
          r.Id,
          r.Name
        })
        .FirstOrDefaultAsync()
          ?? throw new LgdxNotFound404Exception();
      
      return new RobotCertificateRenewBusinessModel {
        RobotId = robot.Id,
        RobotName = robot.Name,
        RootCertificate = newCertificate.RootCertificate,
        RobotCertificatePrivateKey = newCertificate.RobotCertificatePrivateKey,
        RobotCertificatePublicKey = newCertificate.RobotCertificatePublicKey
      };
    }

  public RootCertificateBusinessModel? GetRootCertificate()
  {
    X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
    store.Open(OpenFlags.OpenExistingOnly);
    X509Certificate2 rootCertificate = store.Certificates.First(c => c.SerialNumber == _lgdxRobotCloudConfiguration.RootCertificateSN);
    return new RootCertificateBusinessModel {
      NotBefore = rootCertificate.NotBefore.ToUniversalTime(),
      NotAfter = rootCertificate.NotAfter.ToUniversalTime(),
      PublicKey = rootCertificate.ExportCertificatePem()
    };
  }
}