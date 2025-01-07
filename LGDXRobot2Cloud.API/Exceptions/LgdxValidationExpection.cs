namespace LGDXRobot2Cloud.API.Exceptions;

public class LgdxValidationExpection(string key, string message) : Exception(message)
{
  public string Key { get; init; } = key;
}