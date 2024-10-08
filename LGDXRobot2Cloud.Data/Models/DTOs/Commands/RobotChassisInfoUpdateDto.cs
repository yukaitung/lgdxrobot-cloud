using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class RobotChassisInfoUpdateDto
{
  [Required]
  public int RobotTypeId { get; set; }

  [Required]
  public double ChassisLX { get; set; }

  [Required]
  public double ChassisLY { get; set; }

  [Required]
  public int ChassisWheelCount { get; set; }

  [Required]
  public double ChassisWheelRadius { get; set; }

  [Required]
  public int BatteryCount { get; set; }

  [Required]
  public double BatteryMaxVoltage { get; set; }

  [Required]
  public double BatteryMinVoltage { get; set; }
}