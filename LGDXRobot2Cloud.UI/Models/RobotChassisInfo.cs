using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.Models;

public class RobotChassisInfo
{
  public int Id { get; set; }

  [Required]
  public int LgdxRobotTypeId { get; set; } = 1;

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

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
