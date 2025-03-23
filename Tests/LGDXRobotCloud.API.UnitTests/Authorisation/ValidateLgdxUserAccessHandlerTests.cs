using System.Security.Claims;
using LGDXRobotCloud.API.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Authorisation;

public class ValidateLgdxUserAccessHandlerTests
{
  private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor = new();

  private AuthorizationHandlerContext GenerateContext(string? scope, string? requestArea = null, string? requestController = null, string? requestMethod = null)
  {
    
    var httpContext = new DefaultHttpContext();
    if (requestArea != null)
    {
      httpContext.Request.RouteValues["area"] = requestArea;
    }
    if (requestController != null)
    {
      httpContext.Request.RouteValues["controller"] = requestController;
    }
    if (requestMethod != null)
    {
      httpContext.Request.Method = requestMethod;
    }
    mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(httpContext);
    var claim = new ClaimsIdentity([]);
    if (scope != null)
    {
      claim.AddClaim(new Claim("scope", scope));
    }
    var user = new ClaimsPrincipal(claim);
    var requirement = new ValidateLgdxUserAccessRequirement();
    return new AuthorizationHandlerContext([requirement], user, null);
  }

  [Theory]
  [InlineData("LGDXRobotCloud.API", "FullAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "FullAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "FullAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "FullAccess", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "Read", "GET")]
  [InlineData("LGDXRobotCloud.API", "Write", "POST")]
  [InlineData("LGDXRobotCloud.API", "Write", "PUT")]
  [InlineData("LGDXRobotCloud.API", "Delete", "DELETE")]
  public void HandleRequirementAsync_CalledWithApplication_ShouldReturnTrue(string application, string access, string method)
  {
    // Arrange
    string scope = $"{application}/{access}";
    var context = GenerateContext(scope, null, null, method);
    var validateLgdxUserAccessHandler = new ValidateLgdxUserAccessHandler(mockHttpContextAccessor.Object);

    // Act
    validateLgdxUserAccessHandler.HandleAsync(context);

    // Assert
    Assert.True(context.HasSucceeded);
    Assert.False(context.HasFailed);
  }

  [Theory] 
  [InlineData("InvalidApplication", "FullAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "InvalidAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "InvalidAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "InvalidAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "InvalidAccess", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "Read", "POST")]
  [InlineData("LGDXRobotCloud.API", "Read", "PUT")]
  [InlineData("LGDXRobotCloud.API", "Read", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "Write", "GET")]
  [InlineData("LGDXRobotCloud.API", "Write", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "Delete", "GET")]
  [InlineData("LGDXRobotCloud.API", "Delete", "POST")]
  [InlineData("LGDXRobotCloud.API", "Delete", "PUT")]
  public void HandleRequirementAsync_CalledWithApplicationInvalid_ShouldReturnFalse(string application, string access, string method)
  {
    // Arrange
    string scope = $"{application}/{access}";
    var context = GenerateContext(scope, null, null, method);
    var validateLgdxUserAccessHandler = new ValidateLgdxUserAccessHandler(mockHttpContextAccessor.Object);

    // Act
    validateLgdxUserAccessHandler.HandleAsync(context);

    // Assert
    Assert.False(context.HasSucceeded);
    Assert.True(context.HasFailed);
  }

  [Theory]
  [InlineData("AreaA", "FullAccess", "GET")]
  [InlineData("AreaA", "FullAccess", "POST")]
  [InlineData("AreaA", "FullAccess", "PUT")]
  [InlineData("AreaA", "FullAccess", "DELETE")]
  [InlineData("AreaA", "Read", "GET")]
  [InlineData("AreaA", "Write", "POST")]
  [InlineData("AreaA", "Write", "PUT")]
  [InlineData("AreaA", "Delete", "DELETE")]
  public void HandleRequirementAsync_CalledWithArea_ShouldReturnTrue(string area, string access, string method)
  {
    // Arrange
    string scope = $"LGDXRobotCloud.API/{area}/{access}";
    var context = GenerateContext(scope, area, null, method);
    var validateLgdxUserAccessHandler = new ValidateLgdxUserAccessHandler(mockHttpContextAccessor.Object);

    // Act
    validateLgdxUserAccessHandler.HandleAsync(context);

    // Assert
    Assert.True(context.HasSucceeded);
    Assert.False(context.HasFailed);
  }

  [Theory]
  [InlineData("InvalidApplication", "AreaA", "AreaA", "FullAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "InvalidAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "InvalidAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "InvalidAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "InvalidAccess", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "Read", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "Read", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "Read", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "Write", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "Write", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "Delete", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "Delete", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "AreaA", "Delete", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "FullAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "FullAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "FullAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "FullAccess", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Read", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Read", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Read", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Read", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Write", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Write", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Write", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Write", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Delete", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Delete", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Delete", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "Delete", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "InvalidAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "InvalidAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "InvalidAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "InvalidArea", "InvalidAccess", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "FullAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "FullAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "FullAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "FullAccess", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Read", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Read", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Read", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Read", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Write", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Write", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Write", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Write", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Delete", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Delete", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Delete", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "Delete", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "InvalidAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "InvalidAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "InvalidAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "", "InvalidAccess", "DELETE")]
  public void HandleRequirementAsync_CalledWithAreaInvalid_ShouldReturnFalse(string application, string area, string requestArea, string access, string method)
  {
    // Arrange
    string scope = $"{application}/{area}/{access}";
    var context = GenerateContext(scope, requestArea, null, method);
    var validateLgdxUserAccessHandler = new ValidateLgdxUserAccessHandler(mockHttpContextAccessor.Object);

    // Act
    validateLgdxUserAccessHandler.HandleAsync(context);

    // Assert
    Assert.False(context.HasSucceeded);
    Assert.True(context.HasFailed);
  }

  [Theory]
  [InlineData("ControllerA", "FullAccess", "GET")]
  [InlineData("ControllerA", "FullAccess", "POST")]
  [InlineData("ControllerA", "FullAccess", "PUT")]
  [InlineData("ControllerA", "FullAccess", "DELETE")]
  [InlineData("ControllerA", "Read", "GET")]
  [InlineData("ControllerA", "Write", "POST")]
  [InlineData("ControllerA", "Write", "PUT")]
  [InlineData("ControllerA", "Delete", "DELETE")]
  public void HandleRequirementAsync_CalledWithInvalidController_ShouldReturnTrue(string controller, string access, string method)
  {
    // Arrange
    string scope = $"LGDXRobotCloud.API/AreaA/{controller}/{access}";
    var context = GenerateContext(scope, "AreaA", controller, method);
    var validateLgdxUserAccessHandler = new ValidateLgdxUserAccessHandler(mockHttpContextAccessor.Object);

    // Act
    validateLgdxUserAccessHandler.HandleAsync(context);

    // Assert
    Assert.True(context.HasSucceeded);
    Assert.False(context.HasFailed);
  }

  [Theory]
  [InlineData("InvalidApplication", "AreaA", "ControllerA", "ControllerA", "FullAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "InvalidArea", "ControllerA", "ControllerA", "FullAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "InvalidAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "InvalidAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "InvalidAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "InvalidAccess", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "Read", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "Read", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "Read", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "Write", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "Write", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "Delete", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "Delete", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "ControllerA", "Delete", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "FullAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "FullAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "FullAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "FullAccess", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Read", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Read", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Read", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Read", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Write", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Write", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Write", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Write", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Delete", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Delete", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Delete", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "Delete", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "InvalidAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "InvalidAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "InvalidAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "InvalidController", "InvalidAccess", "DELETE")]  
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "FullAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "FullAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "FullAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "FullAccess", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Read", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Read", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Read", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Read", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Write", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Write", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Write", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Write", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Delete", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Delete", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Delete", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "Delete", "DELETE")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "InvalidAccess", "GET")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "InvalidAccess", "POST")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "InvalidAccess", "PUT")]
  [InlineData("LGDXRobotCloud.API", "AreaA", "ControllerA", "", "InvalidAccess", "DELETE")]  
  public void HandleRequirementAsync_CalledWithInvalidController_ShouldReturnFalse(string application, string area, string controller, string requestController, string access, string method)
  {
    // Arrange
    string scope = $"{application}/{area}/{controller}/{access}";
    var context = GenerateContext(scope, "AreaA", requestController, method);
    var validateLgdxUserAccessHandler = new ValidateLgdxUserAccessHandler(mockHttpContextAccessor.Object);

    // Act
    validateLgdxUserAccessHandler.HandleAsync(context);

    // Assert
    Assert.False(context.HasSucceeded);
    Assert.True(context.HasFailed);
  }
}