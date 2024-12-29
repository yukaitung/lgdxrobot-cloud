namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record RobotListDto
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public required bool IsRealtimeExchange { get; set; }

  public required bool IsProtectingHardwareSerialNumber { get; set; }
}
