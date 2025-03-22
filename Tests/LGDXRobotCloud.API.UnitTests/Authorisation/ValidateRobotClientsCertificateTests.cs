using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.UnitTests.Utilities;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;

namespace LGDXRobotCloud.API.UnitTests.Authorisation;

public class ValidateRobotClientsCertificateTests
{
  private readonly LgdxContext lgdxContext;
  private readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");

  public ValidateRobotClientsCertificateTests()
  {
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
  }

  [Fact]
  public async Task Validate_CalledWithValidCertificateThumbprint_ShouldReturnTrue()
  {
    // Arrange
    var certificate = CertificateHelper.CreateSelfSignedCertificate();
    var robotId = RobotGuid;
    RobotCertificate robotCertificate = new() {
      Id = robotId,
      RobotId = robotId,
      Thumbprint = certificate.Thumbprint,
      ThumbprintBackup = null,
      NotBefore = certificate.NotBefore,
      NotAfter = certificate.NotAfter
    };
    lgdxContext.Set<RobotCertificate>().Add(robotCertificate);
    lgdxContext.SaveChanges();
    var validateRobotClientsCertificate = new ValidateRobotClientsCertificate(lgdxContext);

    // Act
    var result = await validateRobotClientsCertificate.Validate(certificate, robotId);

    // Assert
    Assert.True(result);
  }

  [Fact]
  public async Task Validate_CalledWithValidCertificateThumbprintBackup_ShouldReturnTrue()
  {
    // Arrange
    var certificate = CertificateHelper.CreateSelfSignedCertificate();
    var newCertificate = CertificateHelper.CreateSelfSignedCertificate();
    var robotId = RobotGuid;
    RobotCertificate robotCertificate = new() {
      Id = robotId,
      RobotId = robotId,
      Thumbprint = newCertificate.Thumbprint,
      ThumbprintBackup = certificate.Thumbprint,
      NotBefore = certificate.NotBefore,
      NotAfter = certificate.NotAfter
    };
    lgdxContext.Set<RobotCertificate>().Add(robotCertificate);
    lgdxContext.SaveChanges();
    var validateRobotClientsCertificate = new ValidateRobotClientsCertificate(lgdxContext);

    // Act
    var result = await validateRobotClientsCertificate.Validate(certificate, robotId);

    // Assert
    Assert.True(result);
  }

  [Fact]
  public async Task Validate_CalledWithInvalidCertificateThumbprint_ShouldReturnFalse()
  {
    // Arrange
    var certificate = CertificateHelper.CreateSelfSignedCertificate();
    var robotId = RobotGuid;
    RobotCertificate robotCertificate = new() {
      Id = robotId,
      RobotId = robotId,
      Thumbprint = "Thumbprint",
      ThumbprintBackup = null,
      NotBefore = certificate.NotBefore,
      NotAfter = certificate.NotAfter
    };
    lgdxContext.Set<RobotCertificate>().Add(robotCertificate);
    lgdxContext.SaveChanges();
    var validateRobotClientsCertificate = new ValidateRobotClientsCertificate(lgdxContext);

    // Act
    var result = await validateRobotClientsCertificate.Validate(certificate, robotId);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public async Task Validate_CalledWithInvalidRobotId_ShouldReturnFalse()
  {
    // Arrange
    var certificate = CertificateHelper.CreateSelfSignedCertificate();
    var validateRobotClientsCertificate = new ValidateRobotClientsCertificate(lgdxContext);

    // Act
    var result = await validateRobotClientsCertificate.Validate(certificate, Guid.Empty);

    // Assert
    Assert.False(result);
  }
}