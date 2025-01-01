namespace LGDXRobot2Cloud.Data.Models.Emails;

public record RobotStuckViewModel
{
  public required string RobotId { get; set; }

  public required string RobotName { get; set; }

  public required string RealmId { get; set; }

  public required string RealmName { get; set; }

  public required int AutoTaskId { get; set; }

  public string AutoTaskName { get; set; } = string.Empty;

  public required string Time { get; set; }

  public required double X { get; set; }

  public required double Y { get; set; }
}