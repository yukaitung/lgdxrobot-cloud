using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobotCloud.UI.ViewModels.Navigation;

public class RealmDetailsViewModel : FormViewModel
{
  public int? Id { get; set; } = null;

  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string? Description { get; set; }

  [Required(ErrorMessage = "Please select a traffic control type.")]
  public bool HasWaypointsTrafficControl { get; set; }

  public string? Image { get; set; }

  public IBrowserFile SelectedImage { get; set; } = null!;

  public double? Resolution { get; set; } = null!;

  public double? OriginX { get; set; } = null!;

  public double? OriginY { get; set; } = null!;

  public double? OriginRotation { get; set; } = null!;
}

public static class RealmDetailsViewModelExtensions
{
  public static void FromDto(this RealmDetailsViewModel RealmDetailsViewModel, RealmDto realmDto)
  {
    RealmDetailsViewModel.Id = (int)realmDto.Id!;
    RealmDetailsViewModel.Name = realmDto.Name!;
    RealmDetailsViewModel.Description = realmDto.Description;
    RealmDetailsViewModel.HasWaypointsTrafficControl = (bool)realmDto.HasWaypointsTrafficControl!;
    RealmDetailsViewModel.Image = realmDto.Image!;
    RealmDetailsViewModel.Resolution = realmDto.Resolution;
    RealmDetailsViewModel.OriginX = realmDto.OriginX;
    RealmDetailsViewModel.OriginY = realmDto.OriginY;
    RealmDetailsViewModel.OriginRotation = realmDto.OriginRotation;
  }

  public static RealmUpdateDto ToUpdateDto(this RealmDetailsViewModel RealmDetailsViewModel)
  {
    return new RealmUpdateDto {
      Name = RealmDetailsViewModel.Name,
      Description = RealmDetailsViewModel.Description,
      HasWaypointsTrafficControl = RealmDetailsViewModel.HasWaypointsTrafficControl,
      Image = RealmDetailsViewModel.Image ?? string.Empty,
      Resolution = RealmDetailsViewModel.Resolution ?? 0,
      OriginX = RealmDetailsViewModel.OriginX ?? 0,
      OriginY = RealmDetailsViewModel.OriginY ?? 0,
      OriginRotation = RealmDetailsViewModel.OriginRotation ?? 0
    };
  }

  public static RealmCreateDto ToCreateDto(this RealmDetailsViewModel RealmDetailsViewModel)
  {
    return new RealmCreateDto {
      Name = RealmDetailsViewModel.Name,
      Description = RealmDetailsViewModel.Description,
      HasWaypointsTrafficControl = RealmDetailsViewModel.HasWaypointsTrafficControl,
      Image = RealmDetailsViewModel.Image ?? string.Empty,
      Resolution = RealmDetailsViewModel.Resolution ?? 0,
      OriginX = RealmDetailsViewModel.OriginX ?? 0,
      OriginY = RealmDetailsViewModel.OriginY ?? 0,
      OriginRotation = RealmDetailsViewModel.OriginRotation ?? 0
    };
  }
}