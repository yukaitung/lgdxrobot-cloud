using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.API.UnitTests.Utilities;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Navigation;

public class RobotServiceTests
{
  private static readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
  private static readonly Guid RobotGuid2 = Guid.Parse("0195b00d-42ef-7c57-b2bc-2e015d9a5891");

  private readonly List<Realm> realms = [
    new ()
    {
      Id = 1,
      Name = "Realm 1",
      Image = [],
      Resolution = 1,
      OriginX = 0,
      OriginY = 0,
      OriginRotation = 0
    }
  ];

  private readonly List<Robot> robots = [
    new() {
      Id = RobotGuid,
      Name = "Robot 1",
      RealmId = 1
    },
    new() {
      Id = RobotGuid2,
      Name = "Robot 2",
      RealmId = 1
    }
  ];

  private readonly List<RobotCertificate> robotCertificates = [
    new() {
      Id = RobotGuid,
      RobotId = RobotGuid,
      Thumbprint = "Thumbprint",
      ThumbprintBackup = "ThumbprintBackup",
      NotBefore = DateTime.Now,
      NotAfter = DateTime.Now
    }
  ];

  private readonly List<RobotSystemInfo> robotSystemInfos = [
    new() {
      Id = 1,
      Cpu = "Cpu",
      IsLittleEndian = true,
      Motherboard = "Motherboard",
      MotherboardSerialNumber = "MotherboardSerialNumber",
      RamMiB = 1,
      Gpu = "Gpu",
      Os = "Os",
      Is32Bit = true,
      McuSerialNumber = "McuSerialNumber",
      RobotId = RobotGuid
    }
  ];

  private readonly List<RobotChassisInfo> robotChassisInfos = [
    new() {
      Id = 1,
      RobotId = RobotGuid,
      RobotTypeId = 1,
      ChassisLengthX = 1,
      ChassisLengthY = 1,
      ChassisWheelCount = 1,
      ChassisWheelRadius = 1,
      BatteryCount = 1,
      BatteryMaxVoltage = 1,
      BatteryMinVoltage = 1
    }
  ];

  private readonly List<AutoTask> autoTasks = [
    new() 
    {
      Id = 1,
      Name = "Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.Moving,
    },
  ];

  private readonly Mock<IActivityLogService> mockActivityLogService = new();
  private readonly LgdxContext lgdxContext;
  private readonly Mock<IMemoryCache> mockMemoryCache = new();
  private readonly Mock<IRobotCertificateService> mockRobotCertificateService = new();

  public RobotServiceTests()
  {
    robots[0].AssignedTasks = [autoTasks[0]];
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<Realm>().AddRange(realms);
    lgdxContext.Set<Robot>().AddRange(robots);
    lgdxContext.Set<RobotCertificate>().AddRange(robotCertificates);
    lgdxContext.Set<RobotSystemInfo>().AddRange(robotSystemInfos);
    lgdxContext.Set<RobotChassisInfo>().AddRange(robotChassisInfos);
    lgdxContext.Set<AutoTask>().AddRange(autoTasks);
    lgdxContext.SaveChanges();
  }

  [Theory]
  [InlineData("")]
  [InlineData("Robot")]
  [InlineData("Robot 1")]
  [InlineData("123")]
  public async Task GetRobotsAsync_Called_ShouldReturnsRobots(string robotName)
  {
    // Arrange
    var expected = robots.Where(r => r.Name.Contains(robotName));
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var (actual, _) = await robotService.GetRobotsAsync(1, robotName, 1, robots.Count);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
      Assert.Equal(e.RealmId, a.RealmId);
    });
  }

  [Fact]
  public async Task GetRobotAsync_CalledWithValidId_ShouldReturnsRobot()
  {
    // Arrange
    var expected = robots.Where(r => r.Id == RobotGuid).FirstOrDefault();
    var expectedCertificate = robotCertificates.Where(r => r.RobotId == RobotGuid).FirstOrDefault();
    var expectedRobotSystemInfo = robotSystemInfos.Where(r => r.RobotId == RobotGuid).FirstOrDefault();
    var expectedRobotChassisInfo = robotChassisInfos.Where(r => r.RobotId == RobotGuid).FirstOrDefault();
    var expectedAutoTask = autoTasks.Where(r => r.AssignedRobotId == RobotGuid).FirstOrDefault();
    
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.GetRobotAsync(RobotGuid);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Name, actual.Name);
    Assert.Equal(expected.RealmId, actual.RealmId);
    Assert.Equal(expected.IsRealtimeExchange, actual.IsRealtimeExchange);
    Assert.Equal(expected.IsProtectingHardwareSerialNumber, actual.IsProtectingHardwareSerialNumber);

    Assert.NotNull(actual.RobotCertificate);
    Assert.Equal(expectedCertificate!.Id, actual.Id);
    Assert.Equal(expectedCertificate.Thumbprint, actual.RobotCertificate.Thumbprint);
    Assert.Equal(expectedCertificate.ThumbprintBackup, actual.RobotCertificate.ThumbprintBackup);
    Assert.Equal(expectedCertificate.NotBefore, actual.RobotCertificate.NotBefore);
    Assert.Equal(expectedCertificate.NotAfter, actual.RobotCertificate.NotAfter);

    Assert.NotNull(actual.RobotSystemInfo);
    Assert.Equal(expectedRobotSystemInfo!.Id, actual.RobotSystemInfo!.Id);
    Assert.Equal(expectedRobotSystemInfo.Cpu, actual.RobotSystemInfo.Cpu);
    Assert.Equal(expectedRobotSystemInfo.IsLittleEndian, actual.RobotSystemInfo.IsLittleEndian);
    Assert.Equal(expectedRobotSystemInfo.Motherboard, actual.RobotSystemInfo.Motherboard);
    Assert.Equal(expectedRobotSystemInfo.MotherboardSerialNumber, actual.RobotSystemInfo.MotherboardSerialNumber);
    Assert.Equal(expectedRobotSystemInfo.RamMiB, actual.RobotSystemInfo.RamMiB);
    Assert.Equal(expectedRobotSystemInfo.Gpu, actual.RobotSystemInfo.Gpu);
    Assert.Equal(expectedRobotSystemInfo.Os, actual.RobotSystemInfo.Os);
    Assert.Equal(expectedRobotSystemInfo.Is32Bit, actual.RobotSystemInfo.Is32Bit);
    Assert.Equal(expectedRobotSystemInfo.McuSerialNumber, actual.RobotSystemInfo.McuSerialNumber);

    Assert.NotNull(actual.RobotChassisInfo);
    Assert.Equal(expectedRobotChassisInfo!.Id, actual.RobotChassisInfo!.Id);
    Assert.Equal(expectedRobotChassisInfo.RobotTypeId, actual.RobotChassisInfo.RobotTypeId);
    Assert.Equal(expectedRobotChassisInfo.ChassisLengthX, actual.RobotChassisInfo.ChassisLengthX);
    Assert.Equal(expectedRobotChassisInfo.ChassisLengthY, actual.RobotChassisInfo.ChassisLengthY);
    Assert.Equal(expectedRobotChassisInfo.ChassisWheelCount, actual.RobotChassisInfo.ChassisWheelCount);
    Assert.Equal(expectedRobotChassisInfo.ChassisWheelRadius, actual.RobotChassisInfo.ChassisWheelRadius);
    Assert.Equal(expectedRobotChassisInfo.BatteryCount, actual.RobotChassisInfo.BatteryCount);
    Assert.Equal(expectedRobotChassisInfo.BatteryMaxVoltage, actual.RobotChassisInfo.BatteryMaxVoltage);
    Assert.Equal(expectedRobotChassisInfo.BatteryMinVoltage, actual.RobotChassisInfo.BatteryMinVoltage);
  }

  [Fact]
  public async Task GetRobotAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
    // Arrange
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    Task act() => robotService.GetRobotAsync(Guid.Empty);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task CreateRobotAsync_CalledWithValidRobot_ShouldReturnsRobot()
  {
    // Arrange
    var expected = new RobotCreateBusinessModel {
      Name = "Test Robot",
      RealmId = 1,
      IsRealtimeExchange = true,
      IsProtectingHardwareSerialNumber = true,
      RobotChassisInfo = new RobotChassisInfoCreateBusinessModel {
        RobotTypeId = 1,
        ChassisLengthX = 1,
        ChassisLengthY = 1,
        ChassisWheelCount = 1,
        ChassisWheelRadius = 1,
        BatteryCount = 1,
        BatteryMaxVoltage = 1,
        BatteryMinVoltage = 1
      }
    };
    mockRobotCertificateService.Setup(m => m.IssueRobotCertificateAsync(It.IsAny<Guid>())).ReturnsAsync(new RobotCertificateIssueBusinessModel {
      RootCertificate = "RootCertificate",
      RobotCertificatePrivateKey = "RobotCertificatePrivateKey",
      RobotCertificatePublicKey = "RobotCertificatePublicKey",
      RobotCertificateThumbprint = "RobotCertificateThumbprint",
      RobotCertificateNotAfter = DateTime.Now,
      RobotCertificateNotBefore = DateTime.Now
    });
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.CreateRobotAsync(expected);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected.Name, actual.RobotName);
  }

  [Fact]
  public async Task CreateRobotAsync_CalledWithInvalidRealm_ShouldThrowsValidationException()
  {
    // Arrange
    var expected = new RobotCreateBusinessModel {
      Name = "Test Robot",
      RealmId = realms.Count + 1,
      IsRealtimeExchange = true,
      IsProtectingHardwareSerialNumber = true,
      RobotChassisInfo = new RobotChassisInfoCreateBusinessModel {
        RobotTypeId = 1,
        ChassisLengthX = 1,
        ChassisLengthY = 1,
        ChassisWheelCount = 1,
        ChassisWheelRadius = 1,
        BatteryCount = 1,
        BatteryMaxVoltage = 1,
        BatteryMinVoltage = 1
      }
    };
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    Task act() => robotService.CreateRobotAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
  }

  [Fact]
  public async Task TestDeleteRobotAsync_CalledWithValidId_ShouldReturnsTrue()
  {
    // Arrange
    var expected = robots.Where(r => r.Id == RobotGuid2).FirstOrDefault();
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.TestDeleteRobotAsync(RobotGuid2);

    // Assert
    Assert.True(actual);
  }

  [Fact]
  public async Task TestDeleteRobotAsync_CalledWithAutoTaskDependency_ShouldThrowsValidationException()
  {
    // Arrange
    var depeendencies = 1;
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    Task act() => robotService.TestDeleteRobotAsync(RobotGuid);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"This robot has been used by {depeendencies} running/waiting/template tasks.", exception.Message);
  }

  [Fact]
  public async Task GetRobotSystemInfoAsync_CalledWithValidId_ShouldReturnsRobotSystemInfo()
  {
    // Arrange
    var expected = robotSystemInfos.Where(r => r.RobotId == RobotGuid).FirstOrDefault();
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.GetRobotSystemInfoAsync(RobotGuid);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Id, actual.Id);
    Assert.Equal(expected.Cpu, actual.Cpu);
    Assert.Equal(expected.IsLittleEndian, actual.IsLittleEndian);
    Assert.Equal(expected.Motherboard, actual.Motherboard);
    Assert.Equal(expected.MotherboardSerialNumber, actual.MotherboardSerialNumber);
    Assert.Equal(expected.RamMiB, actual.RamMiB);
    Assert.Equal(expected.Gpu, actual.Gpu);
    Assert.Equal(expected.Os, actual.Os);
    Assert.Equal(expected.Is32Bit, actual.Is32Bit);
    Assert.Equal(expected.McuSerialNumber, actual.McuSerialNumber);
  }

  [Fact]
  public async Task CreateRobotSystemInfoAsync_CalledWithValidRobotSystemInfo_ShouldReturnsTrue()
  {
    // Arrange
    var expected = new RobotSystemInfoCreateBusinessModel {
      Cpu = "Cpu",
      IsLittleEndian = true,
      Motherboard = "Motherboard",
      MotherboardSerialNumber = "MotherboardSerialNumber",
      RamMiB = 1,
      Gpu = "Gpu",
      Os = "Os",
      Is32Bit = true,
      McuSerialNumber = "McuSerialNumber",
    };
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.CreateRobotSystemInfoAsync(RobotGuid, expected);

    // Assert
    Assert.True(actual);
  }

  [Theory]
  [InlineData("")]
  [InlineData("Realm")]
  [InlineData("Realm 1")]
  [InlineData("123")]

  public async Task SearchRobotsAsync_CalledWithRobotName_ShouldReturnsRobots(string name)
  {
    // Arrange
    var expected = robots.Where(r => r.Name.Contains(name));
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.SearchRobotsAsync(1, name, null);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      Assert.Contains(name, a.Name);
    });
  }

  [Fact]
  public async Task GetRobotRealmIdAsync_CalledWithValidId_ShouldReturnsRealmId()
  {
    // Arrange
    var expected = realms.FirstOrDefault(r => r.Id == 1);
    var mmc = MockMemoryCacheService.GetMemoryCache(false);
    var robotService = new RobotService(mockActivityLogService.Object, mmc.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.GetRobotRealmIdAsync(RobotGuid);

    // Assert
    Assert.Equal(expected!.Id, actual);
  }

  [Fact]
  public async Task GetRobotRealmIdAsync_CalledWithIdAgain_ShouldReturnsRealmId()
  {
    // Arrange
    var expected = realms.FirstOrDefault(r => r.Id == 1);
    var mmc = MockMemoryCacheService.GetMemoryCache(expected!.Id);
    var robotService = new RobotService(mockActivityLogService.Object, mmc.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.GetRobotRealmIdAsync(RobotGuid);

    // Assert
    Assert.Equal(expected!.Id, actual);
  }

  [Fact]
  public async Task GetRobotRealmIdAsync_CalledWithInvalidId_ShouldReturnsNull()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(false);
    var robotService = new RobotService(mockActivityLogService.Object, mmc.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.GetRobotRealmIdAsync(Guid.Empty);

    // Assert
    Assert.Null(actual);
  }

  [Fact]
  public async Task GetRobotIsRealtimeExchange_CalledWithValidId_ShouldReturnsRealtimeExchange()
  {
    // Arrange
    var expected = robots.Where(r => r.Id == RobotGuid).FirstOrDefault();
    var robotService = new RobotService(mockActivityLogService.Object, mockMemoryCache.Object, mockRobotCertificateService.Object, lgdxContext);

    // Act
    var actual = await robotService.GetRobotIsRealtimeExchange(RobotGuid);

    // Assert
    Assert.Equal(expected!.IsRealtimeExchange, actual);
  }
}