using System.Security.Claims;
using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Services.Navigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Authorisation;

public class RobotClientShouldOnlineHandlerTests
{
  private static readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");

  private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
  private readonly Mock<IOnlineRobotsService> mockOnlineRobotsService = new();

  private AuthorizationHandlerContext GenerateContext(string? robotId)
  {
    var claim = new ClaimsIdentity([]);
    if (robotId != null)
    {
      claim.AddClaim(new Claim(ClaimTypes.NameIdentifier, robotId));
    }
    var httpContext = new DefaultHttpContext();
    httpContext.User.AddIdentity(claim);
    mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(httpContext);
    var user = new ClaimsPrincipal(claim);
    var requirement = new RobotClientShouldOnlineRequirement();
    return new AuthorizationHandlerContext([requirement], user, null);
  }

  [Fact]
  public async Task HandleRequirementAsync_Called_ShouldReturnTrue()
  {
    // Arrange
    mockOnlineRobotsService.Setup(m => m.IsRobotOnlineAsync(RobotGuid)).ReturnsAsync(true);
    var context = GenerateContext(RobotGuid.ToString());
    var robotClientShouldOnlineHandler = new RobotClientShouldOnlineHandler(mockHttpContextAccessor.Object, mockOnlineRobotsService.Object);

    // Act
    await robotClientShouldOnlineHandler.HandleAsync(context);

    // Assert
    Assert.True(context.HasSucceeded);
    Assert.False(context.HasFailed);
  }

  [Fact]
  public async Task HandleRequirementAsync_CalledWithMissingRobotId_ShouldReturnFalse()
  {
    // Arrange
    mockOnlineRobotsService.Setup(m => m.IsRobotOnlineAsync(RobotGuid)).ReturnsAsync(true);
    var context = GenerateContext(null);
    var robotClientShouldOnlineHandler = new RobotClientShouldOnlineHandler(mockHttpContextAccessor.Object, mockOnlineRobotsService.Object);

    // Act
    await robotClientShouldOnlineHandler.HandleAsync(context);

    // Assert
    Assert.False(context.HasSucceeded);
    Assert.True(context.HasFailed);
  }

  [Fact]
  public async Task HandleRequirementAsync_CalledWithInvalidRobotId_ShouldReturnFalse()
  {
    // Arrange
    var context = GenerateContext("InvalidRobotId");
    var robotClientShouldOnlineHandler = new RobotClientShouldOnlineHandler(mockHttpContextAccessor.Object, mockOnlineRobotsService.Object);

    // Act
    await robotClientShouldOnlineHandler.HandleAsync(context);

    // Assert
    Assert.False(context.HasSucceeded);
    Assert.True(context.HasFailed);
  }

  [Fact]
  public async Task HandleRequirementAsync_CalledWithRobotOffline_ShouldReturnFalse()
  {
    // Arrange
    var context = GenerateContext(RobotGuid.ToString());
    var robotClientShouldOnlineHandler = new RobotClientShouldOnlineHandler(mockHttpContextAccessor.Object, mockOnlineRobotsService.Object);

    // Act
    await robotClientShouldOnlineHandler.HandleAsync(context);

    // Assert
    Assert.False(context.HasSucceeded);
    Assert.True(context.HasFailed);
  }
}