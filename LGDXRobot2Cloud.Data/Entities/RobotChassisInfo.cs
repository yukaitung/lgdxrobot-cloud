using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Data.Entities;

[Table("Robot.RobotChassisInfo")]
public class RobotChassisInfo
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(100)]
  public string McuSerialNumber { get; set; } = null!;

  public int RobotTypeId { get; set; }

  public double ChassisLX { get; set; }

  public double ChassisLY { get; set; }

  public int ChassisWheelCount { get; set; }

  public double ChassisWheelRadius { get; set; }

  public int BatteryCount { get; set; }

  public double BatteryMaxVoltage { get; set; }

  public double BatteryMinVoltage { get; set; }

  [ForeignKey("RobotId")]
  public Robot Robot { get; set; } = null!;

  public Guid RobotId { get; set; }

  [Precision(3)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
  [Precision(3)]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
