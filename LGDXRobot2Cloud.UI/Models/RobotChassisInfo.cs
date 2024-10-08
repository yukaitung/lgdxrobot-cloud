namespace LGDXRobot2Cloud.UI.Models;

public class RobotChassisInfo
{
  public int Id { get; set; }

  public double ChassisLX { get; set; }

  public double ChassisLY { get; set; }

  public int ChassisWheelCount { get; set; }

  public double ChassisWheelRadius { get; set; }

  public int BatteryCount { get; set; }

  public double BatteryMaxVoltage { get; set; }

  public double BatteryMinVoltage { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
