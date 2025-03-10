using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Utilities.Enums;
using MassTransit;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Automation;

public class TriggerServiceTests
{
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
      Body = "{}",
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
    }
  ];

  private readonly List<FlowDetail> flowDetails = [
    new()
    {
      Id = 1,
      Order = 0,
      ProgressId = (int)ProgressState.Starting,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      TriggerId = 2,
      FlowId = 1
    },
  ];

  private readonly Mock<IBus> mockBus = new();
  private readonly Mock<IApiKeyService> mockApiKeyService = new();
  private readonly LgdxContext lgdxContext;

  public TriggerServiceTests()
  {
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<Trigger>().AddRange(triggers);
    lgdxContext.Set<ApiKey>().AddRange(apiKeys);
    lgdxContext.Set<Flow>().AddRange(flows);
    lgdxContext.Set<FlowDetail>().AddRange(flowDetails);
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
    var triggerService = new TriggerService(mockBus.Object, lgdxContext, mockApiKeyService.Object);

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
    var triggerService = new TriggerService(mockBus.Object, lgdxContext, mockApiKeyService.Object);

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
    var triggerService = new TriggerService(mockBus.Object, lgdxContext, mockApiKeyService.Object);

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
    var triggerService = new TriggerService(mockBus.Object, lgdxContext, mockApiKeyService.Object);
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
    var triggerService = new TriggerService(mockBus.Object, lgdxContext, mockApiKeyService.Object);

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
    var triggerService = new TriggerService(mockBus.Object, lgdxContext, mockApiKeyService.Object);
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
    var triggerService = new TriggerService(mockBus.Object, lgdxContext, mockApiKeyService.Object);

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
    var triggerService = new TriggerService(mockBus.Object, lgdxContext, mockApiKeyService.Object);

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
    var triggerService = new TriggerService(mockBus.Object, lgdxContext, mockApiKeyService.Object);

    // Act
    var actual = await triggerService.SearchTriggersAsync(name);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      Assert.Contains(name, a.Name);
    });
  }
  }