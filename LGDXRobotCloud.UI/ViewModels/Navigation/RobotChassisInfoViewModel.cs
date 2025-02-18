using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Navigation;

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

public static class RobotChassisInfoViewModelExtensions
{
  public static void FromDto(this RobotChassisInfoViewModel robotChassisInfoViewModel, RobotChassisInfoDto robotChassisInfoDto)
  {
    robotChassisInfoViewModel.RobotTypeId = robotChassisInfoDto.RobotTypeId;
    robotChassisInfoViewModel.ChassisLengthX = robotChassisInfoDto.ChassisLengthX;
    robotChassisInfoViewModel.ChassisLengthY = robotChassisInfoDto.ChassisLengthY;
    robotChassisInfoViewModel.ChassisWheelCount = robotChassisInfoDto.ChassisWheelCount;
    robotChassisInfoViewModel.ChassisWheelRadius = robotChassisInfoDto.ChassisWheelRadius;
    robotChassisInfoViewModel.BatteryCount = robotChassisInfoDto.BatteryCount;
    robotChassisInfoViewModel.BatteryMaxVoltage = robotChassisInfoDto.BatteryMaxVoltage;
    robotChassisInfoViewModel.BatteryMinVoltage = robotChassisInfoDto.BatteryMinVoltage;
  }
  
  public static RobotChassisInfoCreateDto ToCreateDto(this RobotChassisInfoViewModel robotChassisInfoViewModel)
  {
    return new RobotChassisInfoCreateDto {
      RobotTypeId = robotChassisInfoViewModel.RobotTypeId,
      ChassisLengthX = robotChassisInfoViewModel.ChassisLengthX,
      ChassisLengthY = robotChassisInfoViewModel.ChassisLengthY,
      ChassisWheelCount = robotChassisInfoViewModel.ChassisWheelCount,
      ChassisWheelRadius = robotChassisInfoViewModel.ChassisWheelRadius,
      BatteryCount = robotChassisInfoViewModel.BatteryCount,
      BatteryMaxVoltage = robotChassisInfoViewModel.BatteryMaxVoltage,
      BatteryMinVoltage = robotChassisInfoViewModel.BatteryMinVoltage
    };
  }

  public static RobotChassisInfoUpdateDto ToUpdateDto(this RobotChassisInfoViewModel robotChassisInfoViewModel)
  {
    return new RobotChassisInfoUpdateDto {
      RobotTypeId = robotChassisInfoViewModel.RobotTypeId,
      ChassisLengthX = robotChassisInfoViewModel.ChassisLengthX,
      ChassisLengthY = robotChassisInfoViewModel.ChassisLengthY,
      ChassisWheelCount = robotChassisInfoViewModel.ChassisWheelCount,
      ChassisWheelRadius = robotChassisInfoViewModel.ChassisWheelRadius,
      BatteryCount = robotChassisInfoViewModel.BatteryCount,
      BatteryMaxVoltage = robotChassisInfoViewModel.BatteryMaxVoltage,
      BatteryMinVoltage = robotChassisInfoViewModel.BatteryMinVoltage
    };
  }
}