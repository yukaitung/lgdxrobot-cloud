using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Navigation;

public sealed class RobotChassisInfoViewModel : FormViewModel
{
  [Required]
  public int RobotTypeId { get; set; }

  [Required]
  public double ChassisLengthX { get; set; }

  [Required]
  public double ChassisLengthY { get; set; }

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