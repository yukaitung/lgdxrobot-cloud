using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record RobotUpdateDto
{
  [MaxLength(50)]
  [Required]
  public required string Name { get; set; }

  [Required]
  public required int RealmId { get; set; }

  public bool IsRealtimeExchange { get; set; } = false;

  public bool IsProtectingHardwareSerialNumber { get; set; } = false;
}