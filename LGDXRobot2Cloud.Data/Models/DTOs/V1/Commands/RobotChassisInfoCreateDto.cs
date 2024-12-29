using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

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