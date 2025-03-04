using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Utilities.Enums;
using MassTransit;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Automation;

public class AutoTaskServiceTests
{
  private static readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
  private readonly List<AutoTask> autoTasks = [
    new() 
    {
      Id = 1,
      Name = "Template Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.Template,
    },
    new() 
    {
      Id = 2,
      Name = "Waiting Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.Waiting,
    },
    new() 
    {
      Id = 3,
      Name = "Completed Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.Completed,
    },
    new() 
    {
      Id = 4,
      Name = "Aborted Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.Aborted,
    },
    new() 
    {
      Id = 5,
      Name = "PreMoving Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.PreMoving,
    },
    new() 
    {
      Id = 6,
      Name = "Moving Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.Moving,
    },
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
      WaypointId = 1,
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
      Order = 1,
      CustomX = 1,
      AutoTaskId = 2,
      WaypointId = 1,
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
      RealmId = 1
    }
  ];

  private readonly List<Progress> progresses = [
    new ()
    {
      Id = (int)ProgressState.Template,
      Name = "Template",
      System = true,
      Reserved = true
    },
    new ()
    {
      Id = (int)ProgressState.Waiting,
      Name = "Waiting",
      System = true,
      Reserved = true
    },
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
      Id = (int)ProgressState.PreMoving,
      Name = "PreMoving",
      System = true
    },
    new ()
    {
      Id = (int)ProgressState.Moving,
      Name = "Moving",
      System = true
    }
  ];

  private readonly List<Robot> robots = [
    new() {
      Id = RobotGuid,
      Name = "Test Robot 1",
      RealmId = 1
    }
  ];

  private readonly List<Flow> flows = [
    new ()
    {
      Id = 1,
      Name = "Flow 1"
    }
  ];

  private readonly List<Realm> realms = [
    new ()
    {
      Id = 1,
      Name = "Realm1",
      Image = [],
      Resolution = 1,
      OriginX = 0,
      OriginY = 0,
      OriginRotation = 0
    }
  ];

  private readonly Mock<IAutoTaskSchedulerService> mockAutoTaskSchedulerService = new();
  private readonly Mock<IBus> mockBus = new();
  private readonly Mock<IEventService> mockEventSercice = new();
  private readonly Mock<IOnlineRobotsService> mockOnlineRobotService = new();
  private readonly LgdxContext lgdxContext;

  public AutoTaskServiceTests()
  {
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<AutoTask>().AddRange(autoTasks);
    lgdxContext.Set<AutoTaskDetail>().AddRange(autoTaskDetails);
    lgdxContext.Set<Progress>().AddRange(progresses);
    lgdxContext.Set<Flow>().AddRange(flows);
    lgdxContext.Set<Robot>().AddRange(robots);
    lgdxContext.Set<Realm>().AddRange(realms);
    lgdxContext.Set<Waypoint>().AddRange(waypoints);
    lgdxContext.SaveChanges();
  }

  [Theory]
  [InlineData("")]
  [InlineData("Task")]
  [InlineData("Waiting")]
  [InlineData("321")]
  public async Task GetAutoTasksAsync_ShouldReturnsAutoTasks(string autoTaskName)
  {
    // Arrange
    var expected = autoTasks.Where(t => t.Name!.Contains(autoTaskName));
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    var (actual, _) = await autoTaskService.GetAutoTasksAsync(1, autoTaskName, null);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
      Assert.Equal(e.Priority, a.Priority);
      Assert.Equal(e.FlowId, a.FlowId);
      Assert.Equal(e.RealmId, a.RealmId);
      Assert.Equal(e.AssignedRobotId, a.AssignedRobotId);
      Assert.Equal(e.CurrentProgressId, a.CurrentProgressId);
    });
  }

  [Theory]
  [InlineData(AutoTaskCatrgory.Template, ProgressState.Template)]
  [InlineData(AutoTaskCatrgory.Waiting, ProgressState.Waiting)]
  [InlineData(AutoTaskCatrgory.Completed, ProgressState.Completed)]
  [InlineData(AutoTaskCatrgory.Aborted, ProgressState.Aborted)]
  public async Task GetAutoTaskAsync_CalledWithAutoTaskCatrgory_ShouldReturnsAutoTasks(AutoTaskCatrgory autoTaskCatrgory, ProgressState expectedProgressState)
  {
    // Arrange
    var expected = autoTasks.Where(t => t.CurrentProgressId == (int)expectedProgressState);
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    var (actual, _) = await autoTaskService.GetAutoTasksAsync(1, string.Empty, autoTaskCatrgory);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
      Assert.Equal(e.Priority, a.Priority);
      Assert.Equal(e.FlowId, a.FlowId);
      Assert.Equal(e.RealmId, a.RealmId);
      Assert.Equal(e.AssignedRobotId, a.AssignedRobotId);
      Assert.Equal(e.CurrentProgressId, a.CurrentProgressId);
    });
  }

  [Fact]
  public async Task GetAutoTaskAsync_CalledWithRunningAutoTaskCatrgory_ShouldReturnsRunningAutoTasks()
  {
    // Arrange
    var expected = autoTasks.Where(t => t.CurrentProgressId != (int)ProgressState.Template 
      && t.CurrentProgressId != (int)ProgressState.Waiting 
      && t.CurrentProgressId != (int)ProgressState.Completed 
      && t.CurrentProgressId != (int)ProgressState.Aborted);
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    var (actual, _) = await autoTaskService.GetAutoTasksAsync(1, string.Empty, AutoTaskCatrgory.Running);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
      Assert.Equal(e.Priority, a.Priority);
      Assert.Equal(e.FlowId, a.FlowId);
      Assert.Equal(e.RealmId, a.RealmId);
      Assert.Equal(e.AssignedRobotId, a.AssignedRobotId);
      Assert.Equal(e.CurrentProgressId, a.CurrentProgressId);
    });
  }

  [Fact]
  public async Task GetAutoTaskAsync_CalledWithValidId_ShouldReturnsAutoTask()
  {
    // Arrange
    int id = 1;
    var expected = autoTasks.FirstOrDefault(t => t.Id == id);
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    var actual = await autoTaskService.GetAutoTaskAsync(id);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Name, actual.Name);
    Assert.Equal(expected!.Priority, actual.Priority);
    Assert.Equal(expected!.FlowId, actual.FlowId);
    Assert.Equal(expected!.RealmId, actual.RealmId);
    Assert.Equal(expected!.AssignedRobotId, actual.AssignedRobotId);
    Assert.Equal(expected!.CurrentProgressId, actual.CurrentProgressId);
    Assert.All(actual.AutoTaskDetails, a => {
      var e = expected!.AutoTaskDetails.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Order, a.Order);
      Assert.Equal(e.CustomX, a.CustomX);
      Assert.Equal(e.CustomY, a.CustomY);
      Assert.Equal(e.CustomRotation, a.CustomRotation);
      Assert.Equal(e.Waypoint!.Id, a.Waypoint!.Id);
      Assert.Equal(e.Waypoint!.Name, a.Waypoint!.Name);
      Assert.Equal(e.Waypoint!.RealmId, a.Waypoint!.RealmId);
      Assert.Equal(e.Waypoint!.X, a.Waypoint!.X);
      Assert.Equal(e.Waypoint!.Y, a.Waypoint!.Y);
      Assert.Equal(e.Waypoint!.Rotation, a.Waypoint!.Rotation);
      Assert.Equal(e.Waypoint!.IsParking, a.Waypoint!.IsParking);
      Assert.Equal(e.Waypoint!.HasCharger, a.Waypoint!.HasCharger);
      Assert.Equal(e.Waypoint!.IsReserved, a.Waypoint!.IsReserved);
    });
  }

  [Fact]
  public async Task GetAutoTaskAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
     // Arrange
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.GetAutoTaskAsync(autoTasks.Count + 1);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task CreateAutoTaskAsync_CalledWithValidAutoTask_ShouldReturnsAutoTask()
  {
    // Arrange
    var expected = new AutoTaskCreateBusinessModel {
      Name = "Test Task",
      AutoTaskDetails = [new AutoTaskDetailCreateBusinessModel {
        Order = 0,
        CustomX = 32432,
        CustomY = 543,
        CustomRotation = 0.23432,
        WaypointId = 1
      }],
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    var actual = await autoTaskService.CreateAutoTaskAsync(expected);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected.Name, actual.Name);
    Assert.Equal(expected.Priority, actual.Priority);
    Assert.Equal(expected.FlowId, actual.FlowId);
    Assert.Equal(expected.RealmId, actual.RealmId);
    Assert.Equal(expected.AssignedRobotId, actual.AssignedRobotId);
    Assert.Single(actual.AutoTaskDetails);
    Assert.All(actual.AutoTaskDetails, a => {
      var e = expected.AutoTaskDetails.FirstOrDefault();
      Assert.NotNull(e);
      Assert.Equal(e.Order, a.Order);
      Assert.Equal(e.CustomX, a.CustomX);
      Assert.Equal(e.CustomY, a.CustomY);
      Assert.Equal(e.CustomRotation, a.CustomRotation);
      Assert.Equal(e.WaypointId, a.Waypoint!.Id);
    });
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Once());
  }

  [Fact]
  public async Task CreateAutoTaskAsync_CalledWithValidAutoTaskIsTemplate_ShouldReturnsAutoTask()
  {
    // Arrange
    var expected = new AutoTaskCreateBusinessModel {
      Name = "Test Task",
      AutoTaskDetails = [new AutoTaskDetailCreateBusinessModel {
        Order = 0,
        CustomX = 32432,
        CustomY = 543,
        CustomRotation = 0.23432,
        WaypointId = 1
      }],
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      IsTemplate = true
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    var actual = await autoTaskService.CreateAutoTaskAsync(expected);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected.Name, actual.Name);
    Assert.Equal(expected.Priority, actual.Priority);
    Assert.Equal(expected.FlowId, actual.FlowId);
    Assert.Equal(expected.RealmId, actual.RealmId);
    Assert.Equal(expected.AssignedRobotId, actual.AssignedRobotId);
    Assert.Single(actual.AutoTaskDetails);
    Assert.All(actual.AutoTaskDetails, a => {
      var e = expected.AutoTaskDetails.FirstOrDefault();
      Assert.NotNull(e);
      Assert.Equal(e.Order, a.Order);
      Assert.Equal(e.CustomX, a.CustomX);
      Assert.Equal(e.CustomY, a.CustomY);
      Assert.Equal(e.CustomRotation, a.CustomRotation);
      Assert.Equal(e.WaypointId, a.Waypoint!.Id);
    });
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskUpdateContract>(), It.IsAny<CancellationToken>()), Times.Never());
  }

  [Fact]
  public async Task CreateAutoTaskAsync_CalledWithInvalidWaypoint_ShouldThrowsValidationExpection()
  {
    // Arrange
    int waypointId = waypoints.Count + 1;
    var expected = new AutoTaskCreateBusinessModel {
      Name = "Test Task",
      AutoTaskDetails = [new AutoTaskDetailCreateBusinessModel {
        Order = 0,
        WaypointId = waypointId
      }],
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
   Task act() => autoTaskService.CreateAutoTaskAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"The Waypoint ID {waypointId} is invalid.", exception.Message);
  }

  [Fact]
  public async Task CreateAutoTaskAsync_CalledWithInvalidFlow_ShouldThrowsValidationExpection()
  {
    // Arrange
    int flowId = flows.Count + 1;
    var expected = new AutoTaskCreateBusinessModel {
      Name = "Test Task",
      AutoTaskDetails = [],
      Priority = 0,
      FlowId = flowId,
      RealmId = 1,
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
   Task act() => autoTaskService.CreateAutoTaskAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"The Flow ID {flowId} is invalid.", exception.Message);
  }

  [Fact]
  public async Task CreateAutoTaskAsync_CalledWithInvalidRealm_ShouldThrowsValidationExpection()
  {
    // Arrange
    int realmId = realms.Count + 1;
    var expected = new AutoTaskCreateBusinessModel {
      Name = "Test Task",
      AutoTaskDetails = [],
      Priority = 0,
      FlowId = 1,
      RealmId = realmId,
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
   Task act() => autoTaskService.CreateAutoTaskAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"The Realm ID {realmId} is invalid.", exception.Message);
  }

  [Fact]
  public async Task CreateAutoTaskAsync_CalledWithInvalidRobotId_ShouldThrowsValidationExpection()
  {
    // Arrange
    int realmId = realms.Count + 1;
    var expected = new AutoTaskCreateBusinessModel {
      Name = "Test Task",
      AutoTaskDetails = [],
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = Guid.Empty
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.CreateAutoTaskAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"Robot ID: {Guid.Empty} is invalid.", exception.Message);
  }

  [Fact]
  public async Task UpdateAutoTaskAsync_CalledWithValidAutoTask_ShouldReturnsTrue()
  {
    // Arrange
    int id = 1;
    var expected = autoTasks.FirstOrDefault(t => t.Id == id);
    var update = new AutoTaskUpdateBusinessModel {
      Name = "Test Task Edited",
      AutoTaskDetails = [],
      Priority = 999,
      FlowId = 1,
      AssignedRobotId = RobotGuid
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    var actual = await autoTaskService.UpdateAutoTaskAsync(id, update);

    // Assert
    Assert.True(actual);
    Assert.Equal(expected!.Name, update.Name);
    Assert.Equal(expected.Priority, update.Priority);
    Assert.Equal(expected.FlowId, update.FlowId);
    Assert.Equal(expected.AssignedRobotId, update.AssignedRobotId);
  }

  [Fact]
  public async Task UpdateAutoTaskAsync_CalledWithInvalidAutoTask_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = autoTasks.Count + 1;
    var update = new AutoTaskUpdateBusinessModel {
      Name = "Test Task Edited",
      AutoTaskDetails = [],
      Priority = 999,
      FlowId = 1,
      AssignedRobotId = RobotGuid
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.UpdateAutoTaskAsync(id, update);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task UpdateAutoTaskAsync_CalledWithNotTemplateAutoTask_ShouldThrowsValidationExpection()
  {
    // Arrange
    int id = 2;
    var update = new AutoTaskUpdateBusinessModel {
      Name = "Test Task Edited",
      AutoTaskDetails = [],
      Priority = 999,
      FlowId = 1,
      AssignedRobotId = RobotGuid
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.UpdateAutoTaskAsync(id, update);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("Only AutoTask Templates are editable.", exception.Message);
  }

  [Fact]
  public async Task UpdateAutoTaskAsync_CalledWithTaskDetailBelongsToAnotherAutoTask_ShouldThrowsValidationExpection()
  {
    // Arrange
    int id = 1;
    int detailId = 3;
    var update = new AutoTaskUpdateBusinessModel {
      Name = "Test Task Edited",
      AutoTaskDetails = [new AutoTaskDetailUpdateBusinessModel {
        Id = detailId,
        Order = 0
      }],
      Priority = 999,
      FlowId = 1,
      AssignedRobotId = RobotGuid
    };
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.UpdateAutoTaskAsync(id, update);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"The Task Detail ID {detailId} is belongs to other Task.", exception.Message);
  }

  [Fact]
  public async Task DeleteAutoTaskAsync_CalledWithValidAutoTask_ShouldReturnsTrue()
  {
    // Arrange
    int id = 1;
    
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    var actual = await autoTaskService.DeleteAutoTaskAsync(id);

    // Assert
    var expected = lgdxContext.AutoTasks.FirstOrDefault(t => t.Id == id);
    Assert.True(actual);
    Assert.Null(expected);
  }

  [Fact]
  public async Task DeleteAutoTaskAsync_CalledWithInvalidAutoTask_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = autoTasks.Count + 1;
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.DeleteAutoTaskAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Theory]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  [InlineData(5)]
  public async Task DeleteAutoTaskAsync_CalledWithNonTemplateAutoTask_ShouldThrowsValidationExpection(int id)
  {
    // Arrange
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.DeleteAutoTaskAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("Cannot delete the task not in running status.", exception.Message);
  }

  [Fact]
  public async Task AbortAutoTaskAsync_CalledWithRunningAutoTask_ShouldCalled()
  {
    // Arrange
    int id = 5;
    mockOnlineRobotService.Setup(m => m.SetAbortTaskAsync(It.IsAny<Guid>(), true)).Returns(Task.FromResult(true));
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    await autoTaskService.AbortAutoTaskAsync(id);

    // Assert
    mockOnlineRobotService.Verify(m => m.SetAbortTaskAsync(It.IsAny<Guid>(), true), Times.Once());
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskAbortApiAsync(It.IsAny<int>()), Times.Never());
  }

  [Fact]
  public async Task AbortAutoTaskAsync_CalledWithWaitingAutoTask_ShouldCalled()
  {
    // Arrange
    int id = 2;
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    await autoTaskService.AbortAutoTaskAsync(id);

    // Assert
    mockOnlineRobotService.Verify(m => m.SetAbortTaskAsync(It.IsAny<Guid>(), true), Times.Never());
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskAbortApiAsync(It.IsAny<int>()), Times.Once());
  }

  [Fact]
  public async Task AbortAutoTaskAsync_CalledWithInvalidAutoTask_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = autoTasks.Count + 1;
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.AbortAutoTaskAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(3)]
  [InlineData(4)]
  public async Task AbortAutoTaskAsync_CalledWithStaticAutoTasks_ShouldThrowsValidationExpection(int id)
  {
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.AbortAutoTaskAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("Cannot abort the task not in running status.", exception.Message);
  }

  [Fact]
  public async Task AutoTaskNextApiAsync_CalledWithValidToken_ShouldCalled()
  {
    // Arrange
    AutoTask? autoTask = lgdxContext.AutoTasks.FirstOrDefault(t => t.CurrentProgressId == (int)ProgressState.Moving);
    mockAutoTaskSchedulerService.Setup(m => m.AutoTaskNextApiAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>())).Returns(Task.FromResult(autoTask));
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    await autoTaskService.AutoTaskNextApiAsync((Guid)autoTask!.AssignedRobotId!, autoTask.Id, autoTask.NextToken!);

    // Assert
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskNextApiAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once());
    mockOnlineRobotService.Verify(m => m.SetAutoTaskNextApi(It.IsAny<Guid>(), It.IsAny<AutoTask>()), Times.Once());
  }

  [Fact]
  public async Task AutoTaskNextApiAsync_CalledWithInvalidToken_ShouldThrowsValidationExpection()
  {
    // Arrange
    AutoTask? autoTask = lgdxContext.AutoTasks.FirstOrDefault(t => t.CurrentProgressId == (int)ProgressState.Moving);
    var autoTaskService = new AutoTaskService(lgdxContext, mockAutoTaskSchedulerService.Object, mockBus.Object, mockEventSercice.Object, mockOnlineRobotService.Object);

    // Act
    Task act() => autoTaskService.AutoTaskNextApiAsync((Guid)autoTask!.AssignedRobotId!, autoTask.Id, string.Empty);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("The next token is invalid.", exception.Message);
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskNextApiAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once());
    mockOnlineRobotService.Verify(m => m.SetAutoTaskNextApi(It.IsAny<Guid>(), It.IsAny<AutoTask>()), Times.Never());
  }
}