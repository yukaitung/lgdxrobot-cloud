using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LGDXRobotCloud.API.Exceptions;

public class LgdxNotFound404Exception : Exception, ILgdxExceptionBase
{
  public LgdxNotFound404Exception() : base() { }

  public async Task HandleExceptionAsync(HttpContext context)
  {
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    var problemDetailsFactory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
    var response = problemDetailsFactory.CreateProblemDetails(
      context,
      StatusCodes.Status404NotFound,
      "Not Found"
    );
    await context.Response.WriteAsJsonAsync(response);
  }
}