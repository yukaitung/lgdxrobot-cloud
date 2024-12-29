using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public class RobotChassisInfoCreateDto
{
  [Required]
  public required int RobotTypeId { get; set; }

  [Required]
  public required double ChassisLengthX { get; set; }

  [Required]
  public required double ChassisLengthY { get; set; }

  [Required]
  public required int ChassisWheelCount { get; set; }

  [Required]
  public required double ChassisWheelRadius { get; set; }

  [Required]
  public required int BatteryCount { get; set; }

  [Required]
  public required double BatteryMaxVoltage { get; set; }

  [Required]
  public required double BatteryMinVoltage { get; set; }
}