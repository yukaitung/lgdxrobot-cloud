namespace LGDXRobot2Cloud.Data.Models.Business.Navigation;

public record RobotUpdateBusinessModel
{
  public required string Name { get; set; }

  public required bool IsRealtimeExchange { get; set; }

  public required bool IsProtectingHardwareSerialNumber { get; set; }
}