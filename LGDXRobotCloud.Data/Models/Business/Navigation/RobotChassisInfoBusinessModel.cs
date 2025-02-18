using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record RobotChassisInfoBusinessModel
{
  public int Id { get; set; }

  public required int RobotTypeId { get; set; }

  public required double ChassisLengthX { get; set; }

  public required double ChassisLengthY { get; set; }

  public required int ChassisWheelCount { get; set; }

  public required double ChassisWheelRadius { get; set; }

  public required int BatteryCount { get; set; }

  public required double BatteryMaxVoltage { get; set; }

  public required double BatteryMinVoltage { get; set; }
}

public static class RobotChassisInfoBusinessModelExtensions
{
  public static RobotChassisInfoDto ToDto(this RobotChassisInfoBusinessModel robotChassisInfo)
  {
    return new RobotChassisInfoDto {
      Id = robotChassisInfo.Id,
      RobotTypeId = robotChassisInfo.RobotTypeId,
      ChassisLengthX = robotChassisInfo.ChassisLengthX,
      ChassisLengthY = robotChassisInfo.ChassisLengthY,
      ChassisWheelCount = robotChassisInfo.ChassisWheelCount,
      ChassisWheelRadius = robotChassisInfo.ChassisWheelRadius,
      BatteryCount = robotChassisInfo.BatteryCount,
      BatteryMaxVoltage = robotChassisInfo.BatteryMaxVoltage,
      BatteryMinVoltage = robotChassisInfo.BatteryMinVoltage,
    };
  }
}