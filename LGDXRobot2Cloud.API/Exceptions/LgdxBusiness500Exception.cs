using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LGDXRobot2Cloud.API.Exceptions;

public class LgdxBusiness500Exception(string message) : Exception(message), ILgdxExceptionBase
{
    public async Task HandleExceptionAsync(HttpContext context)
  {
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    var problemDetailsFactory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
    var response = problemDetailsFactory.CreateProblemDetails(
      context,
      StatusCodes.Status500InternalServerError,
      detail: Message
    );
    await context.Response.WriteAsJsonAsync(response);
  }
}