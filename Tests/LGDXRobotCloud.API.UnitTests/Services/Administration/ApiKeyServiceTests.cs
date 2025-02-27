using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Administration;

public class ApiKeyServiceTests
{
  private readonly List<ApiKey> testData = [
    new() {
      Id = 1,
      Name = "Test API Key 1",
      Secret = "Test Secret 1",
      IsThirdParty = false
    },
    new() {
      Id = 2,
      Name = "Test API Key 2",
      Secret = "Test Secret 2",
      IsThirdParty = false
    },
    new() {
      Id = 3,
      Name = "Test API Key 3",
      Secret = "Test Secret 3",
      IsThirdParty = true
    },
    new() {
      Id = 4,
      Name = "Test API Key 4",
      Secret = "Test Secret 4",
      IsThirdParty = true
    }
  ];

  private readonly Mock<DbSet<ApiKey>> mockSet;
  private readonly Mock<LgdxContext> mockContext;

  public ApiKeyServiceTests()
  {
    mockSet = testData.AsQueryable().BuildMockDbSet();
    var optionsBuilder = new DbContextOptionsBuilder<LgdxContext>();
    mockContext = new Mock<LgdxContext>(optionsBuilder.Options);
    mockContext.Setup(c => c.ApiKeys).Returns(() => mockSet.Object);
  }

  [Fact]
  public async Task GetApiKeysAsync_CalledWithLgdxApiKey_ShouldReturnLgdxApiKeys()
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var expected = testData.Where(a => a.IsThirdParty == false).ToList();

    // Act
    var (apiKeys, _) = await apiKeyService.GetApiKeysAsync(null, false, 0, 10);

    // Assert
    Assert.Equal(expected.Count, apiKeys.Count());
    Assert.All(apiKeys, a => {
      var expected = testData.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(expected);
      Assert.Equal(expected.Name, a.Name);
      Assert.False(expected.IsThirdParty);
    });
  }

  [Fact]
  public async Task GetApiKeysAsync_CalledWithThirdPartyApiKey_ShouldReturnThirdPartyApiKeys()
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var expected = testData.Where(a => a.IsThirdParty == true).ToList();

    // Act
    var (apiKeys, _) = await apiKeyService.GetApiKeysAsync(null, true, 0, 10);

    // Assert
    Assert.Equal(expected.Count, apiKeys.Count());
    Assert.All(apiKeys, a => {
      var expected = testData.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(expected);
      Assert.Equal(expected.Name, a.Name);
      Assert.True(expected.IsThirdParty);
    });
  }

  [Theory]
  [InlineData("")]
  [InlineData("Test")]
  [InlineData("Test API Key 1")]
  [InlineData("AAA")]
  public async Task GetApiKeysAsync_CalledWithName_ShouldReturnApiKeysWithName(string name)
  {
    // Arrange
    bool isThirdParty = false;
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var expected = testData.Where(a => a.Name.Contains(name) && a.IsThirdParty == isThirdParty).ToList();

    // Act
    var (apiKeys, _) = await apiKeyService.GetApiKeysAsync(name, isThirdParty, 0, 10);

    // Assert
    Assert.Equal(expected.Count, apiKeys.Count());
    Assert.All(apiKeys, a => {
      Assert.Contains(name, a.Name);
      Assert.Equal(isThirdParty, a.IsThirdParty);
    });
  }

  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  public async Task GetApiKeyAsync_CalledWithApiKeyId_ShouldReturnApiKey(int apiKeyId)
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var expected = testData.First(a => a.Id == apiKeyId);

    // Act
    var apiKey = await apiKeyService.GetApiKeyAsync(apiKeyId);

    // Assert
    Assert.Equal(expected.Id, apiKey.Id);
    Assert.Equal(expected.Name, apiKey.Name);
    Assert.Equal(expected.IsThirdParty, apiKey.IsThirdParty);
  }

  [Fact]
  public async Task GetApiKeyAsync_CalledWithInvalidApiKeyId_ShouldReturnLgdxNotFound404Exception()
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);

    // Act
    Task act() => apiKeyService.GetApiKeyAsync(testData.Count + 1);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  public async Task GetApiKeySecretAsync_CalledWithApiKeyId_ShouldReturnApiKeySecret(int apiKeyId)
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);

    // Act
    var apiKeySecret = await apiKeyService.GetApiKeySecretAsync(apiKeyId);

    // Assert
    Assert.Equal(testData.First(a => a.Id == apiKeyId).Secret, apiKeySecret.Secret);
  }

  [Fact]
  public async Task GetApiKeySecretAsync_CalledWithInvalidApiKeyId_ShouldReturnLgdxNotFound404Exception()
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);

    // Act
    Task act() => apiKeyService.GetApiKeySecretAsync(testData.Count + 1);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Theory]
  [InlineData(true)]
  [InlineData(false)]
  public async Task AddApiKeyAsync_CalledWithApiKey_ShouldReturnApiKey(bool isThirdParty)
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var apiKeyCreateBusinessModel = new ApiKeyCreateBusinessModel {
      Name = "Test API Key",
      IsThirdParty = isThirdParty
    };

    // Act
    var apiKey = await apiKeyService.AddApiKeyAsync(apiKeyCreateBusinessModel);

    // Assert
    Assert.Equal(apiKeyCreateBusinessModel.Name, apiKey.Name);
    Assert.Equal(apiKeyCreateBusinessModel.IsThirdParty, apiKey.IsThirdParty);
  }

  [Theory]
  [InlineData(3, "New Secret 3")]
  [InlineData(4, "New Secret 4")]
  public async Task UpdateApiKeySecretAsync_CalledWithThirdPartyApiKey_ShouldReturnTrue(int apiKeyId, string secret)
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var apiKeyUpdateBusinessModel = new ApiKeySecretUpdateBusinessModel {
      Secret = secret
    };

    // Act
    var isUpdated = await apiKeyService.UpdateApiKeySecretAsync(apiKeyId, apiKeyUpdateBusinessModel);

    // Assert
    var expected = testData.First(a => a.Id == apiKeyId);
    Assert.Equal(secret, expected.Secret);
  }

  [Fact]
  public async Task UpdateApiKeySecretAsync_CalledWithInvalidApiKeyId_ShouldReturnLgdxNotFound404Exception()
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var apiKeyUpdateBusinessModel = new ApiKeySecretUpdateBusinessModel {
      Secret = "New Secret"
    };

    // Act
    Task act() => apiKeyService.UpdateApiKeySecretAsync(testData.Count + 1, apiKeyUpdateBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task UpdateApiKeySecretAsync_CalledWithLgdxApiKey_ShouldReturnLgdxValidation400Expection()
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var apiKeyUpdateBusinessModel = new ApiKeySecretUpdateBusinessModel {
      Secret = "New Secret"
    };

    // Act
    Task act() => apiKeyService.UpdateApiKeySecretAsync(1, apiKeyUpdateBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
  }

  [Theory]
  [InlineData("")]
  [InlineData("Test")]
  [InlineData("Test API Key 1")]
  [InlineData("Test API Key 3")]
  [InlineData("AAA")]
  public async Task SearchApiKeysAsync_CalledWithName_ShouldReturnApiKeysWithName(string name)
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var expected = testData.Where(a => a.Name.Contains(name)).Where(a => a.IsThirdParty == true).ToList();

    // Act
    var apiKeys = await apiKeyService.SearchApiKeysAsync(name);

    // Assert
    Assert.Equal(expected.Count, apiKeys.Count());
    Assert.All(apiKeys, a => Assert.Contains(name, a.Name));
  }

  [Fact]
  public async Task SearchApiKeysAsync_CalledWithoutName_ShouldReturnApiKeys()
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);
    var expected = testData.Where(a => a.IsThirdParty == true).ToList();

    // Act
    var apiKeys = await apiKeyService.SearchApiKeysAsync(null);

    // Assert
    Assert.Equal(expected.Count, apiKeys.Count());
    Assert.All(apiKeys, a => Assert.Contains(a.Name, expected.Select(e => e.Name)));
  }
}