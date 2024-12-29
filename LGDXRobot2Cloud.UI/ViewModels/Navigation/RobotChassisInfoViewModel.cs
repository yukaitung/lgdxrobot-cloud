using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Navigation;

public sealed class RobotChassisInfoViewModel : FormViewModel
{
  [Required (ErrorMessage = "Please select a robot type.")]
  public int? RobotTypeId { get; set; } = null;

  [Required (ErrorMessage = "Please enter a chassis length X.")]
  public double? ChassisLengthX { get; set; } = null;

  [Required (ErrorMessage = "Please enter a chassis length Y.")]
  public double? ChassisLengthY { get; set; } = null;

  [Required (ErrorMessage = "Please enter a chassis wheel count.")]
  public int? ChassisWheelCount { get; set; } = null;

  [Required (ErrorMessage = "Please enter a chassis wheel radius.")]
  public double? ChassisWheelRadius { get; set; } = null;

  [Required (ErrorMessage = "Please enter a battery count.")]
  public int? BatteryCount { get; set; } = null;

  [Required (ErrorMessage = "Please enter a battery max voltage.")]
  public double? BatteryMaxVoltage { get; set; } = null;

  [Required (ErrorMessage = "Please enter a battery min voltage.")]
  public double? BatteryMinVoltage { get; set; } = null;
}