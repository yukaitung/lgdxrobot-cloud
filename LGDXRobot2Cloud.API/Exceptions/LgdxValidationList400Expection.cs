using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LGDXRobot2Cloud.API.Exceptions;

public class LgdxIdentity400Expection(IEnumerable<IdentityError> errors) : Exception(), ILgdxExceptionBase
{
  public async Task HandleExceptionAsync(HttpContext context)
  {
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = StatusCodes.Status400BadRequest;
    ModelStateDictionary modelState = new();
    foreach (var error in errors)
    {
      modelState.AddModelError(error.Code, error.Description);
    }
    var problemDetailsFactory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
    var response = problemDetailsFactory.CreateValidationProblemDetails(
      context,
      modelState
    );
    await context.Response.WriteAsJsonAsync(response);
  }
}