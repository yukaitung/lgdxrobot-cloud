using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobotCloud.Data.Entities;

[Table("Navigation.RobotChassisInfos")]
public class RobotChassisInfo
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  public int RobotTypeId { get; set; }

  public double ChassisLengthX { get; set; }

  public double ChassisLengthY { get; set; }

  public int ChassisWheelCount { get; set; }

  public double ChassisWheelRadius { get; set; }

  public int BatteryCount { get; set; }

  public double BatteryMaxVoltage { get; set; }

  public double BatteryMinVoltage { get; set; }

  [ForeignKey("RobotId")]
  public Robot Robot { get; set; } = null!;

  public Guid RobotId { get; set; }
}
