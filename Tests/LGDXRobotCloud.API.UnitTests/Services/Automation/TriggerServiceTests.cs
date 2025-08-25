using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Data.Models.RabbitMQ;
using LGDXRobotCloud.Utilities.Enums;
using Moq;
using Wolverine;

namespace LGDXRobotCloud.API.UnitTests.Services.Automation;

public class TriggerServiceTests
{
  private static readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");

  private readonly List<Trigger> triggers = [
    new ()
    {
      Id = 1,
      Name = "Trigger 1",
      Url = "https://www.example.com",
      HttpMethodId = (int)TriggerHttpMethod.Post,
      Body = "{}",
      ApiKeyInsertLocationId = (int)ApiKeyInsertLocation.Header,
      ApiKeyFieldName = "x-api-key",
      ApiKeyId = 1,
    },
    new ()
    {
      Id = 2,
      Name = "Trigger 2",
      Url = "https://www.example.com",
      HttpMethodId = (int)TriggerHttpMethod.Post,
      Body = """{"AutoTaskId":"((1))","AutoTaskName":"((2))","AutoTaskCurrentProgressId":"((3))","AutoTaskCurrentProgressName":"((4))","RobotId":"((5))","RobotName":"((6))","RealmId":"((7))","RealmName":"((8))"}""",
      ApiKeyInsertLocationId = (int)ApiKeyInsertLocation.Header,
      ApiKeyFieldName = "x-api-key",
      ApiKeyId = 1,
    }
  ];

  private readonly List<ApiKey> apiKeys = [
    new()
    {
      Id = 1,
      Name = "API Key 1",
      IsThirdParty = true,
      Secret = "secret 1"
    },
    new()
    {
      Id = 2,
      Name = "API Key 2",
      IsThirdParty = false,
      Secret = "secret 2"
    }
  ];

  private readonly List<Flow> flows = [
    new()
    {
      Id = 1,
      Name = "Flow 1"
    },
    new()
    {
      Id = 2,
      Name = "Flow 2"
    }
  ];

  private readonly List<FlowDetail> flowDetails = [
    new()
    {
      Id = 1,
      Order = 0,
      ProgressId = (int)ProgressState.Starting,
      AutoTaskNextControllerId = (int)AutoTaskNextController.API,
      TriggerId = 2,
      FlowId = 1
    },
    new()
    {
      Id = 2,
      Order = 0,
      ProgressId = (int)ProgressState.Starting,
      AutoTaskNextControllerId = (int)AutoTaskNextController.API,
      TriggerId = 999,
      FlowId = 2
    },
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
      CurrentProgressId = (int)ProgressState.Starting,
      NextToken = "123"
    },
    new() 
    {
      Id = 2,
      Name = "Task 2",
      Priority = 0,
      FlowId = 2,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.Starting,
      NextToken = "321"
    }
  ];

  private readonly List<Robot> robots = [
    new() {
      Id = RobotGuid,
      Name = "Test Robot 1",
      RealmId = 1
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

  private readonly Mock<IActivityLogService> mockActivityLogService = new();
  private readonly Mock<IMessageBus> mockBus = new();
  private readonly Mock<IApiKeyService> mockApiKeyService = new();
  private readonly LgdxContext lgdxContext;

  public TriggerServiceTests()
  {
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<Trigger>().AddRange(triggers);
    lgdxContext.Set<ApiKey>().AddRange(apiKeys);
    lgdxContext.Set<Flow>().AddRange(flows);
    lgdxContext.Set<FlowDetail>().AddRange(flowDetails);
    lgdxContext.Set<Robot>().AddRange(robots);
    lgdxContext.Set<Realm>().AddRange(realms);
    lgdxContext.Set<AutoTask>().AddRange(autoTasks);
    lgdxContext.SaveChanges();
  }

  [Theory]
  [InlineData("")]
  [InlineData("Trigger")]
  [InlineData("Trigger 1")]
  [InlineData("123")]
  public async Task GetTriggersAsync_Called_ShouldReturnsTriggers(string triggerName)
  {
    // Arrange
    var expected = triggers.Where(t => t.Name.Contains(triggerName));
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);

    // Act
    var (actual, _) = await triggerService.GetTriggersAsync(triggerName, 1, triggers.Count);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a=> {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
      Assert.Equal(e.Url, a.Url);
      Assert.Equal(e.HttpMethodId, a.HttpMethodId);
    });
  }

  [Fact]
  public async Task GetTriggerAsync_CalledWithValidId_ShouldReturnsTrigger()
  {
    // Arrange
    int id = 1;
    var expected = triggers.Where(t => t.Id == id).FirstOrDefault();
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);

    // Act
    var actual = await triggerService.GetTriggerAsync(id);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Id, actual.Id);
    Assert.Equal(expected!.Name, actual.Name);
    Assert.Equal(expected!.Url, actual.Url);
    Assert.Equal(expected!.HttpMethodId, actual.HttpMethodId);
    Assert.Equal(expected!.Body, actual.Body);
    Assert.Equal(expected.ApiKeyInsertLocationId, actual.ApiKeyInsertLocationId);
    Assert.Equal(expected.ApiKeyFieldName, actual.ApiKeyFieldName);
    Assert.Equal(expected.ApiKeyId, actual.ApiKeyId);
    Assert.NotNull(actual.ApiKeyName);
  }

  [Fact]
  public async Task GetTriggerAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = triggers.Count + 1;
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);

    // Act
    Task act() => triggerService.GetTriggerAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task CreateTriggerAsync_CalledWithValidTrigger_ShouldReturnsTrigger()
  {
    // Arrange
    int apiKeyId = 1;
    var expected = new TriggerCreateBusinessModel {
      Name = "Test Trigger",
      Url = "https://www.example.com",
      HttpMethodId = (int)TriggerHttpMethod.Post,
      Body = "{}",
      ApiKeyInsertLocationId = (int)ApiKeyInsertLocation.Header,
      ApiKeyFieldName = "x-api-key",
      ApiKeyId = apiKeyId,
    };
    var apiKey = apiKeys.Where(a => a.Id == apiKeyId).FirstOrDefault();
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);
    mockApiKeyService.Setup(s => s.GetApiKeyAsync(It.IsAny<int>())).Returns(Task.FromResult(new ApiKeyBusinessModel {
        Id = apiKey!.Id,
        IsThirdParty = apiKey.IsThirdParty,
        Name = apiKey.Name
    }));

    // Act
    var actual = await triggerService.CreateTriggerAsync(expected);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Name, actual.Name);
    Assert.Equal(expected!.Url, actual.Url);
    Assert.Equal(expected!.HttpMethodId, actual.HttpMethodId);
    Assert.Equal(expected!.Body, actual.Body);
    Assert.Equal(expected.ApiKeyInsertLocationId, actual.ApiKeyInsertLocationId);
    Assert.Equal(expected.ApiKeyFieldName, actual.ApiKeyFieldName);
    Assert.Equal(expected.ApiKeyId, actual.ApiKeyId);
    Assert.NotNull(actual.ApiKeyName);
  }

  [Fact]
  public async Task CreateTriggerAsync_CalledWithInvalidApiKeyId_ShouldThrowsValidationException()
  {
    // Arrange
    int apiKeyId = apiKeys.Count;
    var expected = new TriggerCreateBusinessModel {
      Name = "Test Trigger",
      Url = "https://www.example.com",
      HttpMethodId = (int)TriggerHttpMethod.Post,
      Body = "{}",
      ApiKeyInsertLocationId = (int)ApiKeyInsertLocation.Header,
      ApiKeyFieldName = "x-api-key",
      ApiKeyId = apiKeyId,
    };
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);

    // Act
    Task act() => triggerService.CreateTriggerAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"The API Key Id {apiKeyId} is invalid.", exception.Message);
  }

  [Fact]
  public async Task CreateTriggerAsync_CalledWithLgdxApiKey_ShouldThrowsValidationException()
  {
    // Arrange
    int apiKeyId = 2;
    var expected = new TriggerCreateBusinessModel {
      Name = "Test Trigger",
      Url = "https://www.example.com",
      HttpMethodId = (int)TriggerHttpMethod.Post,
      Body = "{}",
      ApiKeyInsertLocationId = (int)ApiKeyInsertLocation.Header,
      ApiKeyFieldName = "x-api-key",
      ApiKeyId = apiKeyId,
    };
    var apiKey = apiKeys.Where(a => a.Id == apiKeyId).FirstOrDefault();
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);
    mockApiKeyService.Setup(s => s.GetApiKeyAsync(It.IsAny<int>())).Returns(Task.FromResult(new ApiKeyBusinessModel {
        Id = apiKey!.Id,
        IsThirdParty = apiKey.IsThirdParty,
        Name = apiKey.Name
    }));

    // Act
    Task act() => triggerService.CreateTriggerAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("Only third party API key is allowed.", exception.Message);
  }

  [Fact]
  public async Task TestDeleteTriggerAsync_CalledWithValidTriggerId_ShouldReturnsTrue()
  {
    // Arrange
    int id = 1;
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);

    // Act
    var actual = await triggerService.TestDeleteTriggerAsync(id);

    // Assert
    Assert.True(actual);
  }

  [Fact]
  public async Task TestDeleteTriggerAsync_CalledWithFlowDetailDependencies_ShouldReturnsValidationExpection()
  {
    // Arrange
    int dependencies = 1;
    int id = 2;
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);

    // Act
    Task act() => triggerService.TestDeleteTriggerAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"This trigger has been used by {dependencies} details in flows.", exception.Message);
  }

  [Theory]
  [InlineData("")]
  [InlineData("Trigger")]
  [InlineData("Trigger 1")]
  [InlineData("123")]
  public async Task SearchTriggersAsync_CalledWithName_ShouldReturnsTriggersWithName(string name)
  {
    // Arrange
    var expected = triggers.Where(p => p.Name.Contains(name));
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);

    // Act
    var actual = await triggerService.SearchTriggersAsync(name);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      Assert.Contains(name, a.Name);
    });
  }

  [Fact]
  public async Task InitialiseTriggerAsync_Called_ShouldPublishTrigger()
  {
    // Arrange
    var task = autoTasks.Where(t => t.Id == 1).FirstOrDefault();
    task!.CurrentProgress = new Progress{
      Id = (int)ProgressState.Starting,
      Name = "Starting",
      System = true
    };
    var flowDetail = flowDetails.Where(t => t.Id == 1).FirstOrDefault();
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);
    var busParam = new List<AutoTaskTriggerRequest>();
    //mockBus.Setup(m => m.PublishAsync(Capture.In(busParam), It.IsAny<CancellationToken>()));
    Dictionary<string, string> expected = new() {
      { "AutoTaskId", task.Id.ToString() },
      { "AutoTaskName", task.Name! },
      { "AutoTaskCurrentProgressId", task.CurrentProgressId.ToString() },
      { "AutoTaskCurrentProgressName", task.CurrentProgress.Name! },
      { "RobotId", RobotGuid.ToString() },
      { "RobotName", robots.Where(r => r.Id == task.AssignedRobotId).FirstOrDefault()!.Name },
      { "RealmId", task.RealmId.ToString() },
      { "RealmName", realms.Where(r => r.Id == task.RealmId).FirstOrDefault()!.Name },
      { "NextToken", task.NextToken! }
    };

    // Act
    await triggerService.InitialiseTriggerAsync(task!, flowDetail!);

    // Assert
    //mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskTriggerRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    Assert.Single(busParam);
    Assert.All(busParam[0].Body, a => {
      Assert.True(expected.ContainsKey(a.Key));
      Assert.Equal(expected[a.Key], a.Value);
    });
    Assert.Equal(expected["AutoTaskId"], busParam[0].AutoTaskId.ToString());
    Assert.Equal(expected["AutoTaskName"], busParam[0].AutoTaskName);
    Assert.Equal(expected["RobotId"], busParam[0].RobotId.ToString());
    Assert.Equal(expected["RobotName"], busParam[0].RobotName);
    Assert.Equal(expected["RealmId"], busParam[0].RealmId.ToString());
    Assert.Equal(expected["RealmName"], busParam[0].RealmName);
  }
/*
  [Fact]
  public async Task InitialiseTriggerAsync_CalledWithInvalidTrigger_ShouldNotPublishTrigger()
  {
    // Arrange
    var task = autoTasks.Where(t => t.Id == 2).FirstOrDefault();
    task!.CurrentProgress = new Progress{
      Id = (int)ProgressState.Starting,
      Name = "Starting",
      System = true
    };
    var flowDetail = flowDetails.Where(t => t.Id == 2).FirstOrDefault();
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);

    // Act
    await triggerService.InitialiseTriggerAsync(task!, flowDetail!);

    // Assert
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskTriggerRequest>(), It.IsAny<CancellationToken>()), Times.Never());
  }

  [Fact]
  public async Task RetryTriggerAsync_Called_ShouldPublishTrigger()
  {
    // Arrange
    var task = autoTasks.Where(t => t.Id == 1).FirstOrDefault();
    task!.CurrentProgress = new Progress{
      Id = (int)ProgressState.Starting,
      Name = "Starting",
      System = true
    };
    var trigger = triggers.Where(t => t.Id == 2).FirstOrDefault();
    var triggerService = new TriggerService(mockActivityLogService.Object, mockApiKeyService.Object, mockBus.Object, lgdxContext);

    // Act
    await triggerService.RetryTriggerAsync(task!, trigger!, "{}");

    // Assert
    mockBus.Verify(m => m.Publish(It.IsAny<AutoTaskTriggerRequest>(), It.IsAny<CancellationToken>()), Times.Once());
  }*/
}