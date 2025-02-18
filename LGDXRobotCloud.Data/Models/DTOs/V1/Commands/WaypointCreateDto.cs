using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record WaypointCreateDto
{
  [MaxLength(100)]
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  [Required (ErrorMessage = "A realm is required.")]
  public required int RealmId { get; set; }
  
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

public static class WaypointCreateDtoExtensions
{
  public static WaypointCreateBusinessModel ToBusinessModel(this WaypointCreateDto model)
  {
    return new WaypointCreateBusinessModel {
      Name = model.Name,
      RealmId = model.RealmId,
      X = model.X,
      Y = model.Y,
      Rotation = model.Rotation,
      IsParking = model.IsParking,
      HasCharger = model.HasCharger,
      IsReserved = model.IsReserved,
    };
  }
}