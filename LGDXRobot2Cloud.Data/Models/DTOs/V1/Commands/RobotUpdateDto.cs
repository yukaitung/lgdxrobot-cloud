using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Navigation;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record RobotUpdateDto
{
  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  public bool IsRealtimeExchange { get; set; } = false;

  public bool IsProtectingHardwareSerialNumber { get; set; } = false;
}

public static class RobotUpdateDtoExtensions
{
  public static RobotUpdateBusinessModel ToBusinessModel(this RobotUpdateDto model)
  {
    return new RobotUpdateBusinessModel {
      Name = model.Name,
      IsRealtimeExchange = model.IsRealtimeExchange,
      IsProtectingHardwareSerialNumber = model.IsProtectingHardwareSerialNumber,
    };
  }
}