using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Administration;

public class ApiKeyServiceTests
{
  private readonly List<ApiKey> testData = [
    new() {
      Id = 1,
      Name = "Test API Key",
      IsThirdParty = false
    },
    new() {
      Id = 2,
      Name = "Test API Key 2",
      IsThirdParty = false
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
  public async Task GetApiKeysAsync_ShouldReturnApiKeys()
  {
    // Arrange
    var apiKeyService = new ApiKeyService(mockContext.Object);

    // Act
    var (apiKeys, PaginationHelper) = await apiKeyService.GetApiKeysAsync(null, false, 0, 10);

    // Assert
    Assert.NotEmpty(apiKeys);
  }
}