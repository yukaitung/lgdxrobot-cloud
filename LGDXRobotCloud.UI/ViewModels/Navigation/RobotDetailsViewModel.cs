using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Navigation;

public class RobotDetailsViewModel : FormViewModelBase
{
  public Guid Id { get; set; }

  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  [Required (ErrorMessage = "Please select a realm.")]
  public int? RealmId { get; set; } = null;

  public string? RealmName { get; set; }
  
  public bool IsProtectingHardwareSerialNumber { get; set; }
}

public static class RobotDetailsViewModelExtensions
{
  public static void FromDto(this RobotDetailsViewModel RobotDetailsViewModel, RobotDto robotDto)
  {
    RobotDetailsViewModel.Id = (Guid)robotDto.Id!;
    RobotDetailsViewModel.Name = robotDto.Name!;
    RobotDetailsViewModel.RealmId = robotDto.Realm!.Id;
    RobotDetailsViewModel.RealmName = robotDto.Realm!.Name;
    RobotDetailsViewModel.IsProtectingHardwareSerialNumber = (bool)robotDto.IsProtectingHardwareSerialNumber!;
  }

  public static RobotCreateDto ToCreateDto(this RobotDetailsViewModel RobotDetailsViewModel, RobotChassisInfoCreateDto robotChassisInfoCreateDto)
  {
    return new RobotCreateDto {
      Name = RobotDetailsViewModel.Name,
      RealmId = RobotDetailsViewModel.RealmId,
      IsProtectingHardwareSerialNumber = RobotDetailsViewModel.IsProtectingHardwareSerialNumber,
      RobotChassisInfo = robotChassisInfoCreateDto
    };
  }
  
  public static RobotUpdateDto ToUpdateDto(this RobotDetailsViewModel RobotDetailsViewModel)
  {
    return new RobotUpdateDto {
      Name = RobotDetailsViewModel.Name,
      IsProtectingHardwareSerialNumber = RobotDetailsViewModel.IsProtectingHardwareSerialNumber
    };
  }
}