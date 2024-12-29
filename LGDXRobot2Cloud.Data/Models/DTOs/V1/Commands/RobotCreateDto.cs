using System.ComponentModel.DataAnnotations;

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
  
  public required RobotChassisInfoCreateDto RobotChassisInfo { get; set; }
}
