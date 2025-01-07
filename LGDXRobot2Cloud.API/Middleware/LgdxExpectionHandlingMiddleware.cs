using LGDXRobot2Cloud.API.Exceptions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LGDXRobot2Cloud.API.Middleware;

public class LgdxExpectionHandlingMiddleware(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      await HandleExceptionAsync(context, ex);
    }
  }

  private static void HandleNotFoundExceptionAsync(HttpContext context)
  {
    context.Response.StatusCode = StatusCodes.Status404NotFound;
  }

  private static async Task HandleValidationExceptionAsync(HttpContext context, LgdxValidationExpection ex)
  {
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = StatusCodes.Status400BadRequest;
    ModelStateDictionary modelState = new();
    modelState.AddModelError(ex.Key, ex.Message);
    var problemDetailsFactory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
    var response = problemDetailsFactory.CreateValidationProblemDetails(
      context,
      modelState
    );
    await context.Response.WriteAsJsonAsync(response);
  }

  private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
  {
    switch (ex)
    {
      case LgdxNotFoundException:
        HandleNotFoundExceptionAsync(context);
        break;
      case LgdxValidationExpection:
        await HandleValidationExceptionAsync(context, (LgdxValidationExpection) ex);
        break;
    }
  }
}

public static class LgdxExpectionHandlingMiddlewareExtensions
{
  public static IApplicationBuilder UseLgdxExpectionHandling(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<LgdxExpectionHandlingMiddleware>();
  }
}