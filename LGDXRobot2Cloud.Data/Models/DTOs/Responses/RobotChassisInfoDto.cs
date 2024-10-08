namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;
public class RobotChassisInfoDto
{
  public int Id { get; set; }

  public int RobotTypeId { get; set; }

  public double ChassisLX { get; set; }

  public double ChassisLY { get; set; }

  public double ChassisLR { get; set; }

  public int ChassisWheelCount { get; set; }

  public double ChassisWheelRadius { get; set; }

  public int BatteryCount { get; set; }

  public double BatteryMaxVoltage { get; set; }

  public double RobotChassisInfo { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}