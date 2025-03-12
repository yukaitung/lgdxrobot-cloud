using EntityFrameworkCore.Testing.Moq;
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
    var triggerRetryService = new TriggerRetryService(mockTriggerService.Object, lgdxContext);

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
}