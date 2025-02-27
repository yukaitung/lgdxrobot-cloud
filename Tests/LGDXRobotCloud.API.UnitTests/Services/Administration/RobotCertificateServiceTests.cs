using System.Security.Cryptography.X509Certificates;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Administration;

public class RobotCertificateServiceTests
{
  private readonly List<RobotCertificate> robotCertificatesTestData = [
    new() {
      Id = Guid.Parse("105cc792-9a12-41e8-9f7d-666b4639fb14"),
      Thumbprint = "Test Thumbprint 1",
      ThumbprintBackup = "Test Thumbprint Backup 1",
      NotBefore = DateTime.UtcNow,
      NotAfter = DateTime.UtcNow.AddDays(1),
    },
    new() {
      Id = Guid.Parse("1435fa1a-afea-4573-8627-76101cbc86e3"),
      Thumbprint = "Test Thumbprint 2",
      ThumbprintBackup = "Test Thumbprint Backup 2",
      NotBefore = DateTime.UtcNow,
      NotAfter = DateTime.UtcNow.AddDays(1),
    },
    new() {
      Id = Guid.Parse("a5096ba4-7476-470e-a867-2ea9be0dded5"),
      Thumbprint = "Test Thumbprint 3",
      ThumbprintBackup = "Test Thumbprint Backup 3",
      NotBefore = DateTime.UtcNow,
      NotAfter = DateTime.UtcNow.AddDays(1),
    }
  ];

  private readonly List<Robot> robotsTestData = [
    new() {
      Id = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127"),
      Name = "Test Robot 1",
      IsRealtimeExchange = true,
      IsProtectingHardwareSerialNumber = true
    },
    new() {
      Id = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4"),
      Name = "Test Robot 2",
      IsRealtimeExchange = false,
      IsProtectingHardwareSerialNumber = false
    },
    new() {
      Id = Guid.Parse("6ac8afe8-6532-4c04-b176-1c2cd0dc484e"),
      Name = "Test Robot 3",
      IsRealtimeExchange = true,
      IsProtectingHardwareSerialNumber = true
    }
  ];

  private readonly Mock<DbSet<RobotCertificate>> mockRobotCertificateSet;
  private readonly Mock<DbSet<Robot>> mockRobotSet;
  private readonly Mock<LgdxContext> mockContext;
  private readonly Mock<IOptionsSnapshot<LgdxRobotCloudConfiguration>> mockConfiguration;
  private readonly LgdxRobotCloudConfiguration lgdxRobotCloudConfiguration;

  public RobotCertificateServiceTests()
  {
    X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
    store.Open(OpenFlags.OpenExistingOnly);
    X509Certificate2 rootCertificate = store.Certificates.First(c => c.Issuer == "CN=LGDXRobotTest");
    lgdxRobotCloudConfiguration = new LgdxRobotCloudConfiguration {
      RootCertificateSN = rootCertificate.SerialNumber,
      RobotCertificateValidDay = 1
    };

    for (int i = 0; i < robotCertificatesTestData.Count; i++)
    {
      robotCertificatesTestData[i].Robot = robotsTestData[i];
      robotCertificatesTestData[i].RobotId = robotsTestData[i].Id;
    }

    mockRobotCertificateSet = robotCertificatesTestData.AsQueryable().BuildMockDbSet();
    mockRobotSet = robotsTestData.AsQueryable().BuildMockDbSet();
    var optionsBuilder = new DbContextOptionsBuilder<LgdxContext>();
    mockContext = new Mock<LgdxContext>(optionsBuilder.Options);
    mockContext.Setup(c => c.RobotCertificates).Returns(() => mockRobotCertificateSet.Object);
    mockContext.Setup(c => c.Robots).Returns(() => mockRobotSet.Object);
    mockConfiguration = new Mock<IOptionsSnapshot<LgdxRobotCloudConfiguration>>();
    mockConfiguration.Setup(o => o.Value).Returns(lgdxRobotCloudConfiguration);
  }

  [Fact]
  public async Task GetRobotCertificatesAsync_ShouldReturnLgdxRobotCertificates()
  {
    // Arrange
    var robotCertificateService = new RobotCertificateService(mockContext.Object, mockConfiguration.Object);

    // Act
    var (robotCertificates, _) = await robotCertificateService.GetRobotCertificatesAsync(0, 10);

    // Assert
    Assert.Equal(robotCertificatesTestData.Count, robotCertificates.Count());
    Assert.All(robotCertificates, a => {
      var expected = robotCertificatesTestData.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(expected);
      Assert.Equal(expected.Thumbprint, a.Thumbprint);
      Assert.Equal(expected.ThumbprintBackup, a.ThumbprintBackup);
      Assert.Equal(expected.NotBefore, a.NotBefore);
      Assert.Equal(expected.NotAfter, a.NotAfter);
    });
  }

  [Fact]
  public async Task GetRobotCertificateAsync_CalledWithValidId_ShouldReturnLgdxRobotCertificate()
  {
    // Arrange
    Guid guid = Guid.Parse("105cc792-9a12-41e8-9f7d-666b4639fb14");
    var robotCertificateService = new RobotCertificateService(mockContext.Object, mockConfiguration.Object);
    var expected = robotCertificatesTestData.First(x => x.Id == guid);

    // Act
    var robotCertificate = await robotCertificateService.GetRobotCertificateAsync(guid);

    // Assert
    Assert.Equal(guid, robotCertificate.Id);
    Assert.Equal(expected.RobotId, robotCertificate.RobotId);
    Assert.Equal(expected.Robot.Name, robotCertificate.RobotName);
    Assert.Equal(expected.Thumbprint, robotCertificate.Thumbprint);
    Assert.Equal(expected.ThumbprintBackup, robotCertificate.ThumbprintBackup);
    Assert.Equal(expected.NotBefore, robotCertificate.NotBefore); 
    Assert.Equal(expected.NotAfter, robotCertificate.NotAfter);
  }

  [Fact]
  public async Task GetRobotCertificateAsync_CalledWithInvalidId_ShouldReturnLgdxNotFound404Exception()
  {
    // Arrange
    var robotCertificateService = new RobotCertificateService(mockContext.Object, mockConfiguration.Object);

    // Act
    Task act() => robotCertificateService.GetRobotCertificateAsync(Guid.Empty);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public void IssueRobotCertificate_CalledWithRobotId_ShouldReturnRobotCertificate()
  {
    // Arrange
    Guid guid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
    var robotCertificateService = new RobotCertificateService(mockContext.Object, mockConfiguration.Object);

    // Act
    var robotCertificate = robotCertificateService.IssueRobotCertificate(guid);

    // Assert
    Assert.NotNull(robotCertificate);
    Assert.NotNull(robotCertificate.RootCertificate);
    Assert.NotNull(robotCertificate.RobotCertificatePrivateKey);
    Assert.NotNull(robotCertificate.RobotCertificatePublicKey);
    Assert.NotNull(robotCertificate.RobotCertificateThumbprint);
    Assert.NotEqual(DateTime.MinValue, robotCertificate.RobotCertificateNotBefore);
    Assert.NotEqual(DateTime.MinValue, robotCertificate.RobotCertificateNotAfter);
  }

  [Fact]
  public async Task RenewRobotCertificateAsync_CalledWithValidIdAndRevokeOldCertificateTrue_ShouldReturnRenewedCertificate()
  {
    // Arrange
    Guid guid = Guid.Parse("105cc792-9a12-41e8-9f7d-666b4639fb14");
    var robotCertificateService = new RobotCertificateService(mockContext.Object, mockConfiguration.Object);
    var robotCertificateRenewRequestBusinessModel = new RobotCertificateRenewRequestBusinessModel()
    {
      CertificateId = guid,
      RevokeOldCertificate = true
    };

    // Act
    var robotCertificate = await robotCertificateService.RenewRobotCertificateAsync(robotCertificateRenewRequestBusinessModel);

    // Assert
    Assert.NotNull(robotCertificate);
    Assert.NotNull(robotCertificate.RootCertificate);
    Assert.NotNull(robotCertificate.RobotCertificatePrivateKey);
    Assert.NotNull(robotCertificate.RobotCertificatePublicKey);

    var updatedCertificate = robotCertificatesTestData.FirstOrDefault(x => x.Id == guid);
    Assert.NotNull(updatedCertificate);
    Assert.Null(updatedCertificate.ThumbprintBackup);
    Assert.NotEqual("Test Thumbprint 1", updatedCertificate.Thumbprint);
    Assert.NotEqual(DateTime.UtcNow.AddDays(-1), updatedCertificate.NotBefore);
    Assert.NotEqual(DateTime.UtcNow.AddDays(1), updatedCertificate.NotAfter);
  }

  [Fact]
  public async Task RenewRobotCertificateAsync_CalledWithValidIdAndRevokeOldCertificateFalse_ShouldReturnRenewedCertificate()
  {
      // Arrange
      Guid guid = Guid.Parse("105cc792-9a12-41e8-9f7d-666b4639fb14");
      var robotCertificateService = new RobotCertificateService(mockContext.Object, mockConfiguration.Object);
      var robotCertificateRenewRequestBusinessModel = new RobotCertificateRenewRequestBusinessModel()
      {
        CertificateId = guid,
        RevokeOldCertificate = false
      };

      // Act
      var robotCertificate = await robotCertificateService.RenewRobotCertificateAsync(robotCertificateRenewRequestBusinessModel);

      // Assert
      Assert.NotNull(robotCertificate);
      Assert.NotNull(robotCertificate.RootCertificate);
      Assert.NotNull(robotCertificate.RobotCertificatePrivateKey);
      Assert.NotNull(robotCertificate.RobotCertificatePublicKey);

      var updatedCertificate = robotCertificatesTestData.FirstOrDefault(x => x.Id == guid);
      Assert.NotNull(updatedCertificate);
      Assert.Equal("Test Thumbprint 1", updatedCertificate.ThumbprintBackup);
      Assert.NotEqual("Test Thumbprint 1", updatedCertificate.Thumbprint);
      Assert.NotEqual(DateTime.UtcNow.AddDays(-1), updatedCertificate.NotBefore);
      Assert.NotEqual(DateTime.UtcNow.AddDays(1), updatedCertificate.NotAfter);
  }

  [Fact]
  public async Task RenewRobotCertificateAsync_CalledWithInvalidId_ShouldReturnLgdxNotFound404Exception()
  {
    // Arrange
    var robotCertificateService = new RobotCertificateService(mockContext.Object, mockConfiguration.Object);
    var robotCertificateRenewRequestBusinessModel = new RobotCertificateRenewRequestBusinessModel()
    {
      CertificateId = Guid.Empty,
      RevokeOldCertificate = true
    };

    // Act
    Task act() => robotCertificateService.RenewRobotCertificateAsync(robotCertificateRenewRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public void GetRootCertificate_ShouldReturnRootCertificate()
  {
    // Arrange
    var robotCertificateService = new RobotCertificateService(mockContext.Object, mockConfiguration.Object);

    //Act
    var rootCertificate = robotCertificateService.GetRootCertificate();

    //Assert
    Assert.NotNull(rootCertificate);
    Assert.NotNull(rootCertificate.PublicKey);
    Assert.NotEqual(DateTime.MinValue, rootCertificate.NotBefore);
    Assert.NotEqual(DateTime.MinValue, rootCertificate.NotAfter);
  }
}