using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public class WaypointCreateDto
{
  [MaxLength(100)]
  [Required]
  public required string Name { get; set; }

  [Required]
  public required int RealmId { get; set; }
  
  [Required]
  public required double X { get; set; }

  [Required]
  public required double Y { get; set; }

  [Required]
  public required double Rotation { get; set; }

  [Required]
  public required bool IsParking { get; set; }

  [Required]
  public required bool HasCharger { get; set; }

  [Required]
  public required bool IsReserved { get; set; }
}
