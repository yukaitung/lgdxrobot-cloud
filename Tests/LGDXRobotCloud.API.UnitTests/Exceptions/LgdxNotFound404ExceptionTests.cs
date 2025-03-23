using LGDXRobotCloud.API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LGDXRobotCloud.API.UnitTests.Exceptions;

public class LgdxNotFound404ExceptionTests
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
    var lgdxNotFound404Exception = new LgdxNotFound404Exception();

    // Act
    await lgdxNotFound404Exception.HandleExceptionAsync(context);

    // Assert
    Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    Assert.NotNull(context.Response.Body);
  }
}