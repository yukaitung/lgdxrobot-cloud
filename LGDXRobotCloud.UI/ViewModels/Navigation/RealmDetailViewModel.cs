using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobotCloud.UI.ViewModels.Navigation;

public sealed class RealmDetailViewModel : FormViewModel
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

public static class RealmDetailViewModelExtensions
{
  public static void FromDto(this RealmDetailViewModel realmDetailViewModel, RealmDto realmDto)
  {
    realmDetailViewModel.Id = (int)realmDto.Id!;
    realmDetailViewModel.Name = realmDto.Name!;
    realmDetailViewModel.Description = realmDto.Description;
    realmDetailViewModel.HasWaypointsTrafficControl = (bool)realmDto.HasWaypointsTrafficControl!;
    realmDetailViewModel.Image = realmDto.Image!;
    realmDetailViewModel.Resolution = realmDto.Resolution;
    realmDetailViewModel.OriginX = realmDto.OriginX;
    realmDetailViewModel.OriginY = realmDto.OriginY;
    realmDetailViewModel.OriginRotation = realmDto.OriginRotation;
  }

  public static RealmUpdateDto ToUpdateDto(this RealmDetailViewModel realmDetailViewModel)
  {
    return new RealmUpdateDto {
      Name = realmDetailViewModel.Name,
      Description = realmDetailViewModel.Description,
      HasWaypointsTrafficControl = realmDetailViewModel.HasWaypointsTrafficControl,
      Image = realmDetailViewModel.Image ?? string.Empty,
      Resolution = realmDetailViewModel.Resolution ?? 0,
      OriginX = realmDetailViewModel.OriginX ?? 0,
      OriginY = realmDetailViewModel.OriginY ?? 0,
      OriginRotation = realmDetailViewModel.OriginRotation ?? 0
    };
  }

  public static RealmCreateDto ToCreateDto(this RealmDetailViewModel realmDetailViewModel)
  {
    return new RealmCreateDto {
      Name = realmDetailViewModel.Name,
      Description = realmDetailViewModel.Description,
      HasWaypointsTrafficControl = realmDetailViewModel.HasWaypointsTrafficControl,
      Image = realmDetailViewModel.Image ?? string.Empty,
      Resolution = realmDetailViewModel.Resolution ?? 0,
      OriginX = realmDetailViewModel.OriginX ?? 0,
      OriginY = realmDetailViewModel.OriginY ?? 0,
      OriginRotation = realmDetailViewModel.OriginRotation ?? 0
    };
  }
}