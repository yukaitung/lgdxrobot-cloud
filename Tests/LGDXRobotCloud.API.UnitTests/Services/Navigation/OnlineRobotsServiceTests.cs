using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.API.UnitTests.Utilities;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Protos;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Navigation;

public class OnlineRobotsServiceTests 
{
  private static readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
  private static readonly int RealmId = 1;
  private static string GetOnlineRobotsKey(int realmId) => $"OnlineRobotsService_OnlineRobots_{realmId}";
  private static string GetRobotCommandsKey(Guid robotId) => $"OnlineRobotsService_RobotCommands_{robotId}";

  private readonly Mock<IBus> mockBus = new();
  private readonly Mock<IEmailService> mockEmailService = new();
  private readonly Mock<IEventService> mockEventService = new();
  private readonly Mock<IMemoryCache> mockMemoryCache = new();
  private readonly Mock<IRobotService> mockRobotService = new();

  public OnlineRobotsServiceTests()
  {
    mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>);
    mockRobotService.Setup(m => m.GetRobotRealmIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult((int?)RealmId));
  }

  [Fact]
  public async Task AddRobotAsync_Called_ShouldRegisterTheRobot()
  {
    // Arrange
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);

    // Act
    await service.AddRobotAsync(RobotGuid);

    // Assert
    mockRobotService.Verify(m => m.GetRobotRealmIdAsync(RobotGuid), Times.Once());
  }

  [Fact]
  public async Task RemoveRobotAsync_Called_ShouldUnregisterTheRobot()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(new HashSet<Guid> {RobotGuid});
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);

    // Act
    await service.RemoveRobotAsync(RobotGuid);

    // Assert
    mockRobotService.Verify(m => m.GetRobotRealmIdAsync(RobotGuid), Times.Once());
    mmc.Verify(m => m.Remove(GetRobotCommandsKey(RobotGuid)), Times.Once());
  }

  [Fact]
  public async Task UpdateRobotDataAsync_Called_ShouldPublishTheData()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(new RobotClientsRobotCommands {});
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);
    var data = new RobotClientsExchange{
      RobotStatus = RobotClientsRobotStatus.Running,
      CriticalStatus = new() {
        HardwareEmergencyStop = false,
        SoftwareEmergencyStop = false        
      },
      Position = new() {
        X = 0,
        Y = 0,
        Rotation = 0
      },
      NavProgress = new() {
        Eta = 0,
        Recoveries = 0,
        DistanceRemaining = 0,
        WaypointsRemaining = 0
      }
    };

    // Act
    await service.UpdateRobotDataAsync(RobotGuid, data);

    // Assert
    mockRobotService.Verify(m => m.GetRobotRealmIdAsync(RobotGuid), Times.Once());
    mockBus.Verify(m => m.Publish(It.IsAny<RobotDataContract>(), It.IsAny<CancellationToken>()), Times.Once());
    mockBus.Verify(m => m.Publish(It.IsAny<RobotCommandsContract>(), It.IsAny<CancellationToken>()), Times.Once());
    mockEmailService.Verify(m => m.SendRobotStuckEmailAsync(RobotGuid, data.Position.X, data.Position.Y), Times.Never());
  }

  [Fact]
  public async Task UpdateRobotDataAsync_CalledAgain_ShouldNotPublishTheData()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(true);
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);

    // Act
    await service.UpdateRobotDataAsync(RobotGuid, new RobotClientsExchange {});

    // Assert
    mockRobotService.Verify(m => m.GetRobotRealmIdAsync(RobotGuid), Times.Never());
    mockBus.Verify(m => m.Publish(It.IsAny<RobotDataContract>(), It.IsAny<CancellationToken>()), Times.Never());
    mockBus.Verify(m => m.Publish(It.IsAny<RobotCommandsContract>(), It.IsAny<CancellationToken>()), Times.Never());
    mockEmailService.Verify(m => m.SendRobotStuckEmailAsync(RobotGuid, It.IsAny<double>(), It.IsAny<double>()), Times.Never());
  }

  [Fact]
  public async Task UpdateRobotDataAsync_CalledWithStuckRobot_ShouldSendEmail()
  {
    // Arrange
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);
    var data = new RobotClientsExchange{
      RobotStatus = RobotClientsRobotStatus.Stuck,
      CriticalStatus = new() {
        HardwareEmergencyStop = false,
        SoftwareEmergencyStop = false        
      },
      Position = new() {
        X = 0,
        Y = 0,
        Rotation = 0
      },
      NavProgress = new() {
        Eta = 0,
        Recoveries = 0,
        DistanceRemaining = 0,
        WaypointsRemaining = 0
      }
    };

    // Act
    await service.UpdateRobotDataAsync(RobotGuid, data);

    // Assert
    mockRobotService.Verify(m => m.GetRobotRealmIdAsync(RobotGuid), Times.Once());
    mockEmailService.Verify(m => m.SendRobotStuckEmailAsync(RobotGuid, data.Position.X, data.Position.Y), Times.Once());
  }

  [Fact]
  public async Task UpdateRobotDataAsync_CalledWithStuckRobotAgain_ShouldNotSendEmail()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(true, $"OnlineRobotsService_RobotStuck_{RobotGuid}");
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);
    var data = new RobotClientsExchange{
      RobotStatus = RobotClientsRobotStatus.Stuck,
      CriticalStatus = new() {
        HardwareEmergencyStop = false,
        SoftwareEmergencyStop = false        
      },
      Position = new() {
        X = 0,
        Y = 0,
        Rotation = 0
      },
      NavProgress = new() {
        Eta = 0,
        Recoveries = 0,
        DistanceRemaining = 0,
        WaypointsRemaining = 0
      }
    };

    // Act
    await service.UpdateRobotDataAsync(RobotGuid, data);

    // Assert
    mockRobotService.Verify(m => m.GetRobotRealmIdAsync(RobotGuid), Times.Once());
    mockEmailService.Verify(m => m.SendRobotStuckEmailAsync(RobotGuid, data.Position.X, data.Position.Y), Times.Never());
  }

  [Fact]
  public void GetRobotCommands_Called_ShouldReturnsRobotCommandsKey()
  {
    // Arrange
    var expected = new RobotClientsRobotCommands {};
    var mmc = MockMemoryCacheService.GetMemoryCache(expected);
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);

    // Act
    var result = service.GetRobotCommands(RobotGuid);

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public void GetRobotCommands_CalledWithOfflineRobot_ShouldReturnsNull()
  {
    // Arrange
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);

    // Act
    var result = service.GetRobotCommands(RobotGuid);

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public async Task IsRobotOnlineAsync_CalledWithOnlineRobot_ShouldReturnsTrue()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(new HashSet<Guid> {RobotGuid});
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);

    // Act
    var result = await service.IsRobotOnlineAsync(RobotGuid);

    // Assert
    Assert.True(result);
  }

  [Fact]
  public async Task IsRobotOnlineAsync_CalledWithOfflineRobot_ShouldReturnsFalse()
  {
    // Arrange
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);

    // Act
    var result = await service.IsRobotOnlineAsync(RobotGuid);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public async Task SetAbortTaskAsync_CalledWithOnlineRobot_ShouldReturnsTrue()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(new RobotClientsRobotCommands {});
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);

    // Act
    var result = await service.SetAbortTaskAsync(RobotGuid, true);

    // Assert
    Assert.True(result);
    mockEventService.Verify(m => m.RobotCommandsHasUpdated(RobotGuid), Times.Once());
  }

  [Fact]
  public async Task SetAbortTaskAsync_CalledWithOfflineRobot_ShouldReturnsFalse()
  {
    // Arrange
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);

    // Act
    var result = await service.SetAbortTaskAsync(RobotGuid, true);

    // Assert
    Assert.False(result);
    mockEventService.Verify(m => m.RobotCommandsHasUpdated(RobotGuid), Times.Never());
  }

  [Fact]
  public async Task SetSoftwareEmergencyStopAsync_CalledWithOnlineRobot_ShouldReturnsTrue()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(new RobotClientsRobotCommands {});
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);

    // Act
    var result = await service.SetSoftwareEmergencyStopAsync(RobotGuid, true);

    // Assert
    Assert.True(result);
    mockEventService.Verify(m => m.RobotCommandsHasUpdated(RobotGuid), Times.Once());
  }

  [Fact]
  public async Task SetSoftwareEmergencyStopAsync_CalledWithOfflineRobot_ShouldReturnsFalse()
  {
    // Arrange
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);

    // Act
    var result = await service.SetSoftwareEmergencyStopAsync(RobotGuid, true);

    // Assert
    Assert.False(result);
    mockEventService.Verify(m => m.RobotCommandsHasUpdated(RobotGuid), Times.Never());
  }

  [Fact]
  public async Task SetPauseTaskAssigementAsync_CalledWithOnlineRobot_ShouldReturnsTrue()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(new RobotClientsRobotCommands {});
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);

    // Act
    var result = await service.SetPauseTaskAssigementAsync(RobotGuid, true);

    // Assert
    Assert.True(result);
    mockEventService.Verify(m => m.RobotCommandsHasUpdated(RobotGuid), Times.Once());
  }

  [Fact]
  public async Task SetPauseTaskAssigementAsync_CalledWithOfflineRobot_ShouldReturnsFalse()
  {
    // Arrange
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);

    // Act
    var result = await service.SetPauseTaskAssigementAsync(RobotGuid, true);

    // Assert
    Assert.False(result);
    mockEventService.Verify(m => m.RobotCommandsHasUpdated(RobotGuid), Times.Never());
  }

  [Fact]
  public void GetPauseAutoTaskAssignment_CalledWithOnlineRobot_ShouldReturnsValue()
  {
    var mmc = MockMemoryCacheService.GetMemoryCache(new RobotClientsRobotCommands {PauseTaskAssigement = true});
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);

    // Act
    var result = service.GetPauseAutoTaskAssignment(RobotGuid);

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void GetPauseAutoTaskAssignment_CalledWithOfflineRobot_ShouldReturnsFalse()
  {
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);

    // Act
    var result = service.GetPauseAutoTaskAssignment(RobotGuid);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void SetAutoTaskNextApi_Called_ShouldCalled()
  {
    // Arrange
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);

    // Act
    service.SetAutoTaskNextApi(RobotGuid, new AutoTask{});

    // Assert
    mockEventService.Verify(m => m.RobotHasNextTaskTriggered(RobotGuid), Times.Once());
  }

  [Fact]
  public void GetAutoTaskNextApi_CalledWithAutoTask_ShouldReturnsAutoTask()
  {
    // Arrange
    var mmc = MockMemoryCacheService.GetMemoryCache(new AutoTask{});
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mmc.Object, mockRobotService.Object);

    // Act
    var result = service.GetAutoTaskNextApi(RobotGuid);

    // Assert
    Assert.NotNull(result);
  }

  [Fact]
  public void GetAutoTaskNextApi_CalledWithNull_ShouldReturnsNull()
  {
    // Arrange
    var service = new OnlineRobotsService(mockBus.Object, mockEmailService.Object, mockEventService.Object, mockMemoryCache.Object, mockRobotService.Object);
    
    // Act
    var result = service.GetAutoTaskNextApi(RobotGuid);

    // Assert
    Assert.Null(result);
  }
}