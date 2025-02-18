namespace LGDXRobotCloud.Data.Models.Emails;

public record AutoTaskAbortViewModel
{
  public required string AutoTaskId { get; set; }

  public string AutoTaskName { get; set; } = string.Empty;

  public required string AbortReason { get; set; }

  public required string RobotId { get; set; }

  public required string RobotName { get; set; }

  public required string RealmId { get; set; }

  public required string RealmName { get; set; }

  public required string Time { get; set; }
}