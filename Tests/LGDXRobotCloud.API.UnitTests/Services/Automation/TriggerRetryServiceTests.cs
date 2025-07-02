using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Utilities.Enums;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Automation;

public class TriggerRetryServiceTests
{
  private static readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");

  private readonly List<Trigger> triggers = [
    new ()
    {
      Id = 1,
      Name = "Trigger 1",
      Url = "https://www.example.com",
      HttpMethodId = (int)TriggerHttpMethod.Post,
      Body = "{}"
    }
  ];

  private readonly List<AutoTask> autoTasks = [
    new() 
    {
      Id = 1,
      Name = "Task 1",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.Moving,
    }
  ];

  private readonly List<TriggerRetry> triggerRetries = [
    new() 
    {
      Id = 1,
      TriggerId = 1,
      AutoTaskId = 1,
      Body = "{}",
      CreatedAt = DateTime.UtcNow
    },
    new() 
    {
      Id = 2,
      TriggerId = 1,
      AutoTaskId = 1,
      Body = "{}",
      CreatedAt = DateTime.UtcNow
    },
    new() 
    {
      Id = 3,
      TriggerId = 1,
      AutoTaskId = 1,
      Body = "{}",
      CreatedAt = DateTime.UtcNow
    }
  ];

  private readonly Mock<IActivityLogService> mockActivityLogService = new();
  private readonly Mock<ITriggerService> mockTriggerService = new();
  private readonly LgdxContext lgdxContext;

  public TriggerRetryServiceTests()
  {
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<AutoTask>().AddRange(autoTasks);
    lgdxContext.Set<Trigger>().AddRange(triggers);
    lgdxContext.Set<TriggerRetry>().AddRange(triggerRetries);
    lgdxContext.SaveChanges();
  }

  [Fact]
  public async Task GetTriggerRetriesAsync_Called_ShouldReturnsTriggerRetries()
  {
    // Arrange
    var expected = triggerRetries;
    var triggerRetryService = new TriggerRetryService(mockActivityLogService.Object,mockTriggerService.Object, lgdxContext);

    // Act
    var (actual, _) = await triggerRetryService.GetTriggerRetriesAsync(1, triggerRetries.Count);

    // Assert
    Assert.Equal(expected.Count, actual.Count());
    Assert.All(actual, a => {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.TriggerId, a.TriggerId);
      Assert.Equal(e.AutoTaskId, a.AutoTaskId);
      Assert.Equal(e.CreatedAt, a.CreatedAt);
    });
  }

  [Fact]
  public async Task GetTriggerRetryAsync_CalledWithValidId_ShouldReturnsTriggerRetry()
  {
    // Arrange
    int id = 1;
    var expected = triggerRetries.Where(t => t.Id == id).FirstOrDefault();
    var triggerRetryService = new TriggerRetryService(mockActivityLogService.Object,mockTriggerService.Object, lgdxContext);

    // Act
    var actual = await triggerRetryService.GetTriggerRetryAsync(id);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.TriggerId, actual.TriggerId);
    Assert.Equal(expected!.AutoTaskId, actual.AutoTaskId);
    Assert.Equal(expected!.CreatedAt, actual.CreatedAt);
    Assert.Equal(expected.Body, actual.Body);
  }

  [Fact]
  public async Task GetTriggerRetryAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = triggerRetries.Count + 1;
    var triggerRetryService = new TriggerRetryService(mockActivityLogService.Object,mockTriggerService.Object, lgdxContext);

    // Act
    Task act() => triggerRetryService.GetTriggerRetryAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task RetryTriggerRetryAsync_CalledWithValidId_ShouldCalled()
  {
    // Arrange
    int id = 1;
    var triggerRetryService = new TriggerRetryService(mockActivityLogService.Object,mockTriggerService.Object, lgdxContext);

    // Act
    await triggerRetryService.RetryTriggerRetryAsync(id);

    // Assert
    mockTriggerService.Verify(s => s.RetryTriggerAsync(It.IsAny<AutoTask>(), It.IsAny<Trigger>(), It.IsAny<string>()), Times.Once());
  }

  [Fact]
  public async Task RetryTriggerRetryAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = triggerRetries.Count + 1;
    var triggerRetryService = new TriggerRetryService(mockActivityLogService.Object,mockTriggerService.Object, lgdxContext);

    // Act
    Task act() => triggerRetryService.RetryTriggerRetryAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockTriggerService.Verify(s => s.RetryTriggerAsync(It.IsAny<AutoTask>(), It.IsAny<Trigger>(), It.IsAny<string>()), Times.Never());
  }
}