using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Navigation;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record RobotCreateDto
{
  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  [Required (ErrorMessage = "Please select a realm.")]
  public required int RealmId { get; set; }

  public bool IsRealtimeExchange { get; set; } = false;

  public bool IsProtectingHardwareSerialNumber { get; set; } = false;
}

public static class RobotCreateDtoExtensions
{
  public static RobotCreateBusinessModel ToBusinessModel(this RobotCreateDto model)
  {
    return new RobotCreateBusinessModel {
      Name = model.Name,
      RealmId = model.RealmId,
      IsRealtimeExchange = model.IsRealtimeExchange,
      IsProtectingHardwareSerialNumber = model.IsProtectingHardwareSerialNumber
    };
  }
}