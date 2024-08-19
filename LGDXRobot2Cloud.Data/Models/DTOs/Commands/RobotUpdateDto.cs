using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class RobotUpdateDto
{
  [Required]
  public string Name { get; set; } = null!;

  public string? Address { get; set; }

  public string? Namespace { get; set; }

  public bool IsRealtimeExchange { get; set; }

  public bool IsProtectingHardwareSerialNumber { get; set; }
}