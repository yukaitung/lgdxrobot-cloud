using LGDXRobotCloud.API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LGDXRobotCloud.API.UnitTests.Exceptions;

public class LgdxIdentity400ExpectionTests
{
  [Fact]
  public async Task HandleExceptionAsync_Called_ShouldReturnExpectedStatusCode()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddMvc();
    var provider = services.BuildServiceProvider();
    var context = new DefaultHttpContext
    {
      RequestServices = provider
    };
    var errors = new List<IdentityError>
    {
      new() {
        Code = "code",
        Description = "description"
      }
    };
    var lgdxIdentity400Expection = new LgdxIdentity400Expection(errors);

    // Act
    await lgdxIdentity400Expection.HandleExceptionAsync(context);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    Assert.NotNull(context.Response.Body);
  }
}