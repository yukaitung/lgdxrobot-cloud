namespace LGDXRobot2Cloud.Data.Contracts;

public record RobotCommands
{
  public bool AbortTask { get; set; }
  public bool RenewCertificate { get; set; }
  public bool SoftwareEmergencyStop { get; set; }
  public bool PauseTaskAssigement { get; set; }
}

public record RobotCommandsContract
{
  public required Guid RobotId { get; set; }
  public RobotCommands Commands { get; set; } = new();
}