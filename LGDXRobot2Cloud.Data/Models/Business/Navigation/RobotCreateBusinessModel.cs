namespace LGDXRobot2Cloud.Data.Models.Business.Navigation;

public record RobotCreateBusinessModel
{
  public required string Name { get; set; }

  public required int RealmId { get; set; }

  public required bool IsRealtimeExchange { get; set; }

  public required bool IsProtectingHardwareSerialNumber { get; set; }

  public required RobotChassisInfoCreateBusinessModel RobotChassisInfo { get; set; }
}