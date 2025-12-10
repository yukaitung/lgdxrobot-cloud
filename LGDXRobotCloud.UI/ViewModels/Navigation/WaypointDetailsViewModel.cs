using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Navigation;

public class WaypointDetailsViewModel : FormViewModelBase
{
  public int Id { get; set; }

  [MaxLength(100)]
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  [Required (ErrorMessage = "A realm is required.")]
  public int? RealmId { get; set; } = null;

  public string? RealmName { get; set; }
  
  [Required (ErrorMessage = "Please enter a X coordinate.")]
  public double? X { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a Y coordinate.")]
  public double? Y { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a rotation.")]
  public double? Rotation { get; set; } = null!;

  public bool IsParking { get; set; }

  public bool HasCharger { get; set; }

  public bool IsReserved { get; set; }
}

public static class WaypointDetailsViewModelExtensions
{
  public static void FromDto(this WaypointDetailsViewModel WaypointDetailsViewModel, WaypointDto waypointDto)
  {
    WaypointDetailsViewModel.Id = (int)waypointDto.Id!;
    WaypointDetailsViewModel.Name = waypointDto.Name!;
    WaypointDetailsViewModel.RealmId = waypointDto.Realm!.Id;
    WaypointDetailsViewModel.RealmName = waypointDto.Realm!.Name;
    WaypointDetailsViewModel.X = waypointDto.X;
    WaypointDetailsViewModel.Y = waypointDto.Y;
    WaypointDetailsViewModel.Rotation = waypointDto.Rotation;
    WaypointDetailsViewModel.IsParking = (bool)waypointDto.IsParking!;
    WaypointDetailsViewModel.HasCharger = (bool)waypointDto.HasCharger!;
    WaypointDetailsViewModel.IsReserved = (bool)waypointDto.IsReserved!;
  }

  public static WaypointUpdateDto ToUpdateDto(this WaypointDetailsViewModel WaypointDetailsViewModel)
  {
    return new WaypointUpdateDto {
      Name = WaypointDetailsViewModel.Name,
      X = WaypointDetailsViewModel.X,
      Y = WaypointDetailsViewModel.Y,
      Rotation = WaypointDetailsViewModel.Rotation,
      IsParking = WaypointDetailsViewModel.IsParking,
      HasCharger = WaypointDetailsViewModel.HasCharger,
      IsReserved = WaypointDetailsViewModel.IsReserved
    };
  }

  public static WaypointCreateDto ToCreateDto(this WaypointDetailsViewModel WaypointDetailsViewModel)
  {
    return new WaypointCreateDto {
      Name = WaypointDetailsViewModel.Name,
      RealmId = WaypointDetailsViewModel.RealmId,
      X = WaypointDetailsViewModel.X,
      Y = WaypointDetailsViewModel.Y,
      Rotation = WaypointDetailsViewModel.Rotation,
      IsParking = WaypointDetailsViewModel.IsParking,
      HasCharger = WaypointDetailsViewModel.HasCharger,
      IsReserved = WaypointDetailsViewModel.IsReserved
    };
  }
}