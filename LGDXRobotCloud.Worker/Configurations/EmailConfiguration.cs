namespace LGDXRobotCloud.Worker.Configurations;

public class EmailConfiguration
{
  // SMTP Settings
  public required string FromName { get; set; }

  public required string FromAddress { get; set; } 

  public required string Host { get; set; }

  public required int Port { get; set; }

  public required string Username { get; set; }

  public required string Password { get; set; } 
}