using LGDXRobotCloud.API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LGDXRobotCloud.API.UnitTests.Exceptions;

public class LgdxValidation400ExpectionTests
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
    var lgdxValidation400Expection = new LgdxValidation400Expection("key", "message");

    // Act
    await lgdxValidation400Expection.HandleExceptionAsync(context);

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    Assert.NotNull(context.Response.Body);
  }
}