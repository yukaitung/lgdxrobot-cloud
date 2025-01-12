using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Navigation;

public class WaypointDetailViewModel : FormViewModel
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

public static class WaypointDetailViewModelExtensions
{
  public static void FromDto(this WaypointDetailViewModel waypointDetailViewModel, WaypointDto waypointDto)
  {
    waypointDetailViewModel.Id = (int)waypointDto.Id!;
    waypointDetailViewModel.Name = waypointDto.Name!;
    waypointDetailViewModel.RealmId = waypointDto.Realm!.Id;
    waypointDetailViewModel.RealmName = waypointDto.Realm!.Name;
    waypointDetailViewModel.X = waypointDto.X;
    waypointDetailViewModel.Y = waypointDto.Y;
    waypointDetailViewModel.Rotation = waypointDto.Rotation;
    waypointDetailViewModel.IsParking = (bool)waypointDto.IsParking!;
    waypointDetailViewModel.HasCharger = (bool)waypointDto.HasCharger!;
    waypointDetailViewModel.IsReserved = (bool)waypointDto.IsReserved!;
  }

  public static WaypointUpdateDto ToUpdateDto(this WaypointDetailViewModel waypointDetailViewModel)
  {
    return new WaypointUpdateDto {
      Name = waypointDetailViewModel.Name,
      RealmId = waypointDetailViewModel.RealmId,
      X = waypointDetailViewModel.X,
      Y = waypointDetailViewModel.Y,
      Rotation = waypointDetailViewModel.Rotation,
      IsParking = waypointDetailViewModel.IsParking,
      HasCharger = waypointDetailViewModel.HasCharger,
      IsReserved = waypointDetailViewModel.IsReserved
    };
  }

  public static WaypointCreateDto ToCreateDto(this WaypointDetailViewModel waypointDetailViewModel)
  {
    return new WaypointCreateDto {
      Name = waypointDetailViewModel.Name,
      RealmId = waypointDetailViewModel.RealmId,
      X = waypointDetailViewModel.X,
      Y = waypointDetailViewModel.Y,
      Rotation = waypointDetailViewModel.Rotation,
      IsParking = waypointDetailViewModel.IsParking,
      HasCharger = waypointDetailViewModel.HasCharger,
      IsReserved = waypointDetailViewModel.IsReserved
    };
  }
}