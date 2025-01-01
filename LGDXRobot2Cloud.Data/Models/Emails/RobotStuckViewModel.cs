namespace LGDXRobot2Cloud.Data.Models.Emails;

public record RobotStuckViewModel
{
  public required string RobotId { get; set; }

  public required string RobotName { get; set; }

  public required string RealmId { get; set; }

  public required string RealmName { get; set; }

  public required string Time { get; set; }

  public required string X { get; set; }

  public required string Y { get; set; }
}