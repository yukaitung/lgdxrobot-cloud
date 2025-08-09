using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record RobotCreateDto
{
  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  [Required (ErrorMessage = "Please select a realm.")]
  public required int RealmId { get; set; }

  public bool IsProtectingHardwareSerialNumber { get; set; } = false;

  public required RobotChassisInfoCreateDto RobotChassisInfo { get; set; }
}

public static class RobotCreateDtoExtensions
{
  public static RobotCreateBusinessModel ToBusinessModel(this RobotCreateDto model)
  {
    return new RobotCreateBusinessModel {
      Name = model.Name,
      RealmId = model.RealmId,
      IsProtectingHardwareSerialNumber = model.IsProtectingHardwareSerialNumber,
      RobotChassisInfo = model.RobotChassisInfo.ToBusinessModel()
    };
  }
}