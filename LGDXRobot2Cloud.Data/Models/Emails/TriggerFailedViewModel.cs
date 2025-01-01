namespace LGDXRobot2Cloud.Data.Models.Emails;

public record TriggerFailedViewModel
{
  public required string TriggerId { get; set; }

  public required string TriggerName { get; set; }

  public required string TriggerUrl { get; set; }

  public required string HttpMethodId { get; set; }

  public required string AutoTaskId { get; set; }

  public required string AutoTaskName { get; set; }

  public required string RobotId { get; set; }

  public required string RobotName { get; set; }

  public required string RealmId { get; set; }

  public required string RealmName { get; set; }
}