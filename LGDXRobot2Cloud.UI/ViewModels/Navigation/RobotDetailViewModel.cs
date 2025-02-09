using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Navigation;

public sealed class RobotDetailViewModel : FormViewModel
{
  public Guid Id { get; set; }

  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  [Required (ErrorMessage = "Please select a realm.")]
  public int? RealmId { get; set; } = null;

  public string? RealmName { get; set; }
  
  public bool IsRealtimeExchange { get; set; }

  public bool IsProtectingHardwareSerialNumber { get; set; }
}

public static class RobotDetailViewModelExtensions
{
  public static void FromDto(this RobotDetailViewModel robotDetailViewModel, RobotDto robotDto)
  {
    robotDetailViewModel.Id = (Guid)robotDto.Id!;
    robotDetailViewModel.Name = robotDto.Name!;
    robotDetailViewModel.RealmId = robotDto.Realm!.Id;
    robotDetailViewModel.RealmName = robotDto.Realm!.Name;
    robotDetailViewModel.IsRealtimeExchange = (bool)robotDto.IsRealtimeExchange!;
    robotDetailViewModel.IsProtectingHardwareSerialNumber = (bool)robotDto.IsProtectingHardwareSerialNumber!;
  }

  public static RobotCreateDto ToCreateDto(this RobotDetailViewModel robotDetailViewModel, RobotChassisInfoCreateDto robotChassisInfoCreateDto)
  {
    return new RobotCreateDto {
      Name = robotDetailViewModel.Name,
      RealmId = robotDetailViewModel.RealmId,
      IsRealtimeExchange = robotDetailViewModel.IsRealtimeExchange,
      IsProtectingHardwareSerialNumber = robotDetailViewModel.IsProtectingHardwareSerialNumber,
      RobotChassisInfo = robotChassisInfoCreateDto
    };
  }
  
  public static RobotUpdateDto ToUpdateDto(this RobotDetailViewModel robotDetailViewModel)
  {
    return new RobotUpdateDto {
      Name = robotDetailViewModel.Name,
      IsRealtimeExchange = robotDetailViewModel.IsRealtimeExchange,
      IsProtectingHardwareSerialNumber = robotDetailViewModel.IsProtectingHardwareSerialNumber
    };
  }
}