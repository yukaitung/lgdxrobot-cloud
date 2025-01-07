namespace LGDXRobot2Cloud.API.Exceptions;

public interface ILgdxExceptionBase
{
  public Task HandleExceptionAsync(HttpContext context);
}