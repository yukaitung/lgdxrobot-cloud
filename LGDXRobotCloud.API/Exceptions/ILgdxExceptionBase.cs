namespace LGDXRobotCloud.API.Exceptions;

public interface ILgdxExceptionBase
{
  public Task HandleExceptionAsync(HttpContext context);
}