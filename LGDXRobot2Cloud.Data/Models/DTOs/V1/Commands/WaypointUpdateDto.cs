using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Navigation;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record WaypointUpdateDto
{
  [MaxLength(100)]
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }
  
  [Required (ErrorMessage = "Please enter a X coordinate.")]
  public required double X { get; set; }

  [Required (ErrorMessage = "Please enter a Y coordinate.")]
  public required double Y { get; set; }

  [Required (ErrorMessage = "Please enter a rotation.")]
  public required double Rotation { get; set; }

  public bool IsParking { get; set; } = false;

  public bool HasCharger { get; set; } = false;

  public bool IsReserved { get; set; } = false;
}

public static class WaypointUpdateDtoExtensions
{
  public static WaypointUpdateBusinessModel ToBusinessModel(this WaypointUpdateDto model)
  {
    return new WaypointUpdateBusinessModel {
      Name = model.Name,
      X = model.X,
      Y = model.Y,
      Rotation = model.Rotation,
      IsParking = model.IsParking,
      HasCharger = model.HasCharger,
      IsReserved = model.IsReserved,
    };
  }
}