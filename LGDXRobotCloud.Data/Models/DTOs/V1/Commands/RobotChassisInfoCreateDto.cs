using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record RobotChassisInfoCreateDto
{
  [Required (ErrorMessage = "Please select a robot type.")]
  public required int RobotTypeId { get; set; }

  [Required (ErrorMessage = "Please enter a chassis length X.")]
  public required double ChassisLengthX { get; set; }

  [Required (ErrorMessage = "Please enter a chassis length Y.")]
  public required double ChassisLengthY { get; set; }

  [Required (ErrorMessage = "Please enter a chassis wheel count.")]
  public required int ChassisWheelCount { get; set; }

  [Required (ErrorMessage = "Please enter a chassis wheel radius.")]
  public required double ChassisWheelRadius { get; set; }

  [Required (ErrorMessage = "Please enter a battery count.")]
  public required int BatteryCount { get; set; }

  [Required (ErrorMessage = "Please enter a battery max voltage.")]
  public required double BatteryMaxVoltage { get; set; }

  [Required (ErrorMessage = "Please enter a battery min voltage.")]
  public required double BatteryMinVoltage { get; set; }
}

public static class RobotChassisInfoCreateDtoExtensions
{
  public static RobotChassisInfoCreateBusinessModel ToBusinessModel(this RobotChassisInfoCreateDto model)
  {
    return new RobotChassisInfoCreateBusinessModel {
      RobotTypeId = model.RobotTypeId,
      ChassisLengthX = model.ChassisLengthX,
      ChassisLengthY = model.ChassisLengthY,
      ChassisWheelCount = model.ChassisWheelCount,
      ChassisWheelRadius = model.ChassisWheelRadius,
      BatteryCount = model.BatteryCount,
      BatteryMaxVoltage = model.BatteryMaxVoltage,
      BatteryMinVoltage = model.BatteryMinVoltage,
    };
  }
}