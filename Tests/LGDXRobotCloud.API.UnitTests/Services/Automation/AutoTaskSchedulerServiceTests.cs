using EntityFrameworkCore.Testing.Moq;
using EntityFrameworkCore.Testing.Moq.Extensions;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Utilities.Enums;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Automation;

public class AutoTaskSchedulerServiceTests
{
  private readonly List<AutoTask> autoTasks = [
    new() 
    {
      Id = 1,
      Name = "Waiting Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.Waiting,
    },
    new() 
    {
      Id = 2,
      Name = "Running Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.Moving,
      CurrentProgressOrder = 0,
      NextToken = "Next Token 1",
      AssignedRobotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4")
    },
    new() 
    {
      Id = 3,
      Name = "Running Task Last Step",
      Priority = 0,
      FlowId = 1,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.Completing,
      CurrentProgressOrder = 1,
      NextToken = "Next Token 2",
      AssignedRobotId = Guid.Parse("6ac8afe8-6532-4c04-b176-1c2cd0dc484e")
    }
  ];

  private readonly List<AutoTaskDetail> autoTaskDetails = [
    new()
    {
      Id = 1,
      Order = 0,
      CustomX = 0,
      CustomY = 0,
      CustomRotation = 0,
      AutoTaskId = 1,
    },
    new()
    {
      Id = 2,
      Order = 1,
      CustomX = 1,
      AutoTaskId = 1,
      WaypointId = 1,
    },
    new()
    {
      Id = 3,
      Order = 2,
      CustomY = 1,
      AutoTaskId = 1,
      WaypointId = 2,
    },
    new()
    {
      Id = 4,
      Order = 3,
      CustomRotation = 1,
      AutoTaskId = 1,
      WaypointId = 3,
    },
    new()
    {
      Id = 5,
      Order = 4,
      CustomY = 0,
      CustomRotation = 0,
      AutoTaskId = 1,
    },
    new()
    {
      Id = 6,
      Order = 5,
      CustomX = 0,
      CustomRotation = 0,
      AutoTaskId = 1,
    },
    new()
    {
      Id = 7,
      Order = 6,
      CustomX = 0,
      CustomY = 0,
      AutoTaskId = 1,
    },
  ];

  private readonly List<Progress> progresses = [
    new ()
    {
      Id = (int)ProgressState.Completed,
      Name = "Completed",
      System = true,
      Reserved = true
    },
    new ()
    {
      Id = (int)ProgressState.Aborted,
      Name = "Aborted",
      System = true,
      Reserved = true
    },
    new ()
    {
      Id = (int)ProgressState.Moving,
      Name = "Moving",
      System = true
    },
    new ()
    {
      Id = (int)ProgressState.Completing,
      Name = "Completing",
      System = true
    },
  ];

  private readonly List<Flow> flows = [
    new ()
    {
      Id = 1,
      Name = "Flow 1"
    }
  ];

  private readonly List<FlowDetail> flowDetails = [
    new()
    {
      Id = 1,
      Order = 0,
      ProgressId = (int)ProgressState.Moving,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 1
    },
    new()
    {
      Id = 2,
      Order = 1,
      ProgressId = (int)ProgressState.Completing,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 1
    }
  ];

  private readonly List<Robot> robots = [
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

  private readonly List<Waypoint> waypoints = [
    new()
    {
      Id = 1,
      Name = "1",
      X = 43,
      Y = 56,
      Rotation = 0.233,
    },
    new()
    {
      Id = 2,
      Name = "2",
      X = 54,
      Y = 453,
      Rotation = 0.786,
    },
    new()
    {
      Id = 3,
      Name = "3",
      X = 5,
      Y = 9,
      Rotation = 0,
    },
    new()
    {
      Id = 4,
      Name = "4",
      X = 7,
      Y = 98,
      Rotation = 0.676,
    },
    new()
    {
      Id = 5,
      Name = "5",
      X = 0,
      Y = 0,
      Rotation = 0,
    },
  ];

  private readonly Mock<IBus> mockBus = new();
  private readonly Mock<IEmailService> mockEmailService = new();
  private readonly Mock<IMemoryCache> mockMemoryCache = new();
  private readonly Mock<IOnlineRobotsService> mockOnlineRobotService = new();
  private readonly Mock<IRobotService> mockRobotService = new();
  private readonly Mock<ITriggerService> mockTriggerService = new();
  private readonly LgdxContext lgdxContext;

  public static class MockMemoryCacheService 
  {
    public static Mock<IMemoryCache> GetMemoryCache(object expectedValue) 
    {
      var mockMemoryCache = new Mock<IMemoryCache>();
      mockMemoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedValue)).Returns(true);
      return mockMemoryCache;
    }
  }
  public AutoTaskSchedulerServiceTests()
  {
    mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>);
    for (int i = 0; i < autoTasks.Count; i++)
    {
      var details = autoTaskDetails.Where(a => a.AutoTaskId == autoTasks[i].Id).ToList();
      foreach (var detail in details)
        autoTasks[i].AutoTaskDetails.Add(detail);
    }
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<AutoTask>().AddRange(autoTasks);
    lgdxContext.Set<AutoTaskDetail>().AddRange(autoTaskDetails);
    lgdxContext.Set<Progress>().AddRange(progresses);
    lgdxContext.Set<Flow>().AddRange(flows);
    lgdxContext.Set<FlowDetail>().AddRange(flowDetails);
    lgdxContext.Set<Robot>().AddRange(robots);
    lgdxContext.Set<Waypoint>().AddRange(waypoints);
    lgdxContext.SaveChanges();
  }

  [Fact]
  public void ResetIgnoreRobot_ShouldCalled()
  {
    // Arrange
    int realmId = 0;
    string key = $"AutoTaskSchedulerService_IgnoreRobot_{realmId}";
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    service.ResetIgnoreRobot(realmId);

    // Assert
    mockMemoryCache.Verify(m => m.Remove(key), Times.Once);
  }

  [Fact]
  public async Task GetAutoTaskAsync_CalledWithRobotHasNoAutoTask_ShouldReturnsAutoTask()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Waiting).FirstOrDefault();
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([expected]);
    Guid robotId = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);
    
    // Act
    var actual = await service.GetAutoTaskAsync(robotId);

    // Assert
    Assert.Equal(expected!.Id, actual!.TaskId);
    Assert.Equal(expected.Name, actual.TaskName);
    Assert.Equal((int)ProgressState.Moving, actual.TaskProgressId);
    Assert.Equal("Moving", actual.TaskProgressName);
    Assert.Equal(expected.AutoTaskDetails.Count, actual.Waypoints.Count);
    Assert.NotNull(actual.NextToken);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetAutoTaskAsync_CalledWithDatabaseNoAutoTask_ShouldReturnsNull()
  {
    // Arrange
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([]);
    Guid robotId = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);
    
    // Act
    var actual = await service.GetAutoTaskAsync(robotId);

    // Assert
    Assert.Null(actual);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task GetAutoTaskAsync_CalledWithIgnoredRobot_ShouldReturnsNull()
  {
    // Arrange
    Guid robotId = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
    HashSet<Guid> ignoredRobots = [robotId];
    var mmc = MockMemoryCacheService.GetMemoryCache(ignoredRobots);
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mmc.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);
    
    // Act
    var actual = await service.GetAutoTaskAsync(robotId);

    // Assert
    Assert.Null(actual);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task GetAutoTaskAsync_CalledWithRobotHasRunningTask_ShouldReturnsAutoTask()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.GetAutoTaskAsync(robotId);

    // Assert
    Assert.Equal(expected!.Id, actual!.TaskId);
    Assert.Equal(expected.Name, actual.TaskName);
    Assert.Equal((int)ProgressState.Moving, actual.TaskProgressId);
    Assert.Equal("Moving", actual.TaskProgressName);
    Assert.Equal(expected.AutoTaskDetails.Count, actual.Waypoints.Count);
    Assert.NotNull(actual.NextToken);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task GetAutoTaskAsync_CalledWithGetPauseAutoTaskAssignmentTrue_ShouldReturnsNull()
  {
    // Arrange
    mockOnlineRobotService.Setup(x => x.GetPauseAutoTaskAssignment(It.IsAny<Guid>())).Returns(true);
    Guid robotId = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);
    
    // Act
    var actual = await service.GetAutoTaskAsync(robotId);

    // Assert
    Assert.Null(actual);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task AutoTaskAbortAsync_CalledWithValidToken_ShouldReturnsAutoTask()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([expected]);
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskAbortAsync(robotId, expected!.Id, expected.NextToken!, AutoTaskAbortReason.Robot);

    // Assert
    Assert.Equal(expected!.Id, actual!.TaskId);
    Assert.Equal(expected.Name, actual.TaskName);
    Assert.Equal((int)ProgressState.Aborted, actual.TaskProgressId);
    Assert.Equal("Aborted", actual.TaskProgressName);
    Assert.Empty(actual.Waypoints);
    Assert.Empty(actual.NextToken);
    mockEmailService.Verify(m => m.SendAutoTaskAbortEmailAsync(robotId, expected!.Id, AutoTaskAbortReason.Robot), Times.Once);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task AutoTaskAbortAsync_CalledWithInvalidToken_ShouldReturnsNull()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([]);
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskAbortAsync(robotId, expected!.Id, expected.NextToken!, AutoTaskAbortReason.Robot);

    // Assert
    Assert.Null(actual);
    mockEmailService.Verify(m => m.SendAutoTaskAbortEmailAsync(robotId, expected!.Id, AutoTaskAbortReason.Robot), Times.Never);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task AutoTaskAbortApiAsync_CalledWithValidAutoTaskId_ShouldReturnsTrue()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([expected]);
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskAbortApiAsync(expected!.Id);

    // Assert
    Assert.True(actual);
    mockEmailService.Verify(m => m.SendAutoTaskAbortEmailAsync(robotId, expected!.Id, AutoTaskAbortReason.UserApi), Times.Once);
  }

  [Fact]
  public async Task AutoTaskAbortApiAsync_CalledWithInvalidAutoTaskId_ShouldReturnsFalse()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([]);
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskAbortApiAsync(expected!.Id);

    // Assert
    Assert.False(actual);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task AutoTaskNextAsync_CalledWithRunningTask_ShouldReturnsAutoTask()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    string expectedNextToken = expected!.NextToken!;
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([expected]);
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskNextAsync(robotId, expected!.Id, expected.NextToken!);

    // Assert
    Assert.Equal(expected!.Id, actual!.TaskId);
    Assert.Equal(expected.Name, actual.TaskName);
    Assert.Equal((int)ProgressState.Completing, actual.TaskProgressId);
    Assert.Equal("Completing", actual.TaskProgressName);
    Assert.Empty(actual.Waypoints);
    Assert.NotEqual(expectedNextToken, actual.NextToken);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task AutoTaskNextAsync_CalledWithRunningTaskLastStep_ShouldReturnsAutoTask()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Completing).FirstOrDefault();
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([expected]);
    Guid robotId = Guid.Parse("6ac8afe8-6532-4c04-b176-1c2cd0dc484e");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskNextAsync(robotId, expected!.Id, expected.NextToken!);

    // Assert
    Assert.Equal(expected!.Id, actual!.TaskId);
    Assert.Equal(expected.Name, actual.TaskName);
    Assert.Equal((int)ProgressState.Completed, actual.TaskProgressId);
    Assert.Equal("Completed", actual.TaskProgressName);
    Assert.Empty(actual.Waypoints);
    Assert.Empty(actual.NextToken);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task AutoTaskNextAsync_CalledWithInvalidAutoTask_ShouldReturnsNull()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([]);
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskNextAsync(robotId, expected!.Id, expected.NextToken!);

    // Assert
    Assert.Null(actual);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task AutoTaskNextApiAsync_CalledWithRunningTask_ShouldReturnsAutoTask()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([expected]);
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskNextApiAsync(robotId, expected!.Id, expected!.NextToken!);

    // Assert
    Assert.Equal(expected, actual);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task AutoTaskNextApiAsync_CalledWithInvalidAutoTask_ShouldReturnsNull()
  {
    // Arrange
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    lgdxContext.Set<AutoTask>().AddFromSqlRawResult([]);
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskNextApiAsync(robotId, expected!.Id, expected!.NextToken!);

    // Assert
    Assert.Null(actual);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task GenerateTaskDetail_CalledWithAutoTask_ReturnAutoTask()
  {
    var expected = autoTasks.Where(a => a.CurrentProgressId == (int)ProgressState.Moving).FirstOrDefault();
    Guid robotId = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
    var service = new AutoTaskSchedulerService(mockBus.Object, mockEmailService.Object, mockMemoryCache.Object, mockOnlineRobotService.Object, mockRobotService.Object, mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await service.AutoTaskNextConstructAsync(expected!);

    // Assert
    Assert.Equal(expected!.Id, actual!.TaskId);
    Assert.Equal(expected.Name, actual.TaskName);
    Assert.Equal((int)ProgressState.Moving, actual.TaskProgressId);
    Assert.Equal("Moving", actual.TaskProgressName);
    Assert.Equal(expected.AutoTaskDetails.Count, actual.Waypoints.Count);
    Assert.Equal(expected.NextToken, actual.NextToken);
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Once);
  }
}