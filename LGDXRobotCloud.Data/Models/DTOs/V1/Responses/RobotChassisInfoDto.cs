namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
public record RobotChassisInfoDto
{
  public required int Id { get; set; }

  public required int RobotTypeId { get; set; }

  public required double ChassisLengthX { get; set; }

  public required double ChassisLengthY { get; set; }

  public required int ChassisWheelCount { get; set; }

  public required double ChassisWheelRadius { get; set; }

  public required int BatteryCount { get; set; }

  public required double BatteryMaxVoltage { get; set; }

  public required double BatteryMinVoltage { get; set; }
}