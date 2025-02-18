using LGDXRobotCloud.API.Exceptions;
namespace LGDXRobotCloud.API.Middleware;

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
      if (ex is ILgdxExceptionBase casted)
      {
        await HandleExceptionAsync(context, casted);
      }
      else throw;
    }
  }

  private static async Task HandleExceptionAsync(HttpContext context, ILgdxExceptionBase ex)
  {
    await ex.HandleExceptionAsync(context);
  }
}

public static class LgdxExpectionHandlingMiddlewareExtensions
{
  public static IApplicationBuilder UseLgdxExpectionHandling(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<LgdxExpectionHandlingMiddleware>();
  }
}