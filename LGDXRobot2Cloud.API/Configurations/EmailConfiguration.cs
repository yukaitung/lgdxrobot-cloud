namespace LGDXRobot2Cloud.API.Configurations;

public class EmailConfiguration
{
  public string FromName { get; set; } = string.Empty;
  public string FromAddress { get; set; } = string.Empty;
  public string Host { get; set; } = string.Empty;
  public int Port { get; set; }
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
}