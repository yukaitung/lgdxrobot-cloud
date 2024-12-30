using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record AutoTaskDetailCreateDto
{
  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  
  public int? WaypointId { get; set; }

  [Required]
  public int Order { get; set; }
}
