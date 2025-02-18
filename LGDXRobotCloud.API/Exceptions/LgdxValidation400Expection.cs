using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LGDXRobotCloud.API.Exceptions;

public class LgdxValidation400Expection(string key, string message) : Exception(message), ILgdxExceptionBase
{
  public string Key { get; init; } = key;

  public async Task HandleExceptionAsync(HttpContext context)
  {
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = StatusCodes.Status400BadRequest;
    ModelStateDictionary modelState = new();
    modelState.AddModelError(Key, Message);
    var problemDetailsFactory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
    var response = problemDetailsFactory.CreateValidationProblemDetails(
      context,
      modelState
    );
    await context.Response.WriteAsJsonAsync(response);
  }
}