using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.ViewModels.Shared;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.ViewModels.Navigation;

public sealed class RealmDetailViewModel : FormViewModel, IValidatableObject
{
  public int? Id { get; set; } = null;

  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string? Description { get; set; }

  public string Image { get; set; } = null!;

  public IBrowserFile SelectedImage { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a resolution.")]
  public double? Resolution { get; set; } = null!;

  [Required (ErrorMessage = "Please enter an origin X coordinate.")]
  public double? OriginX { get; set; } = null!;

  [Required (ErrorMessage = "Please enter an origin Y coordinate.")]
  public double? OriginY { get; set; } = null!;

  [Required (ErrorMessage = "Please enter an origin rotation.")]
  public double? OriginRotation { get; set; } = null!;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Id == null && SelectedImage == null)
    {
      yield return new ValidationResult("Please select an image.", [nameof(SelectedImage)]);
    }
  }
}

public static class RealmDetailViewModelExtensions
{
  public static void FromDto(this RealmDetailViewModel realmDetailViewModel, RealmDto realmDto)
  {
    realmDetailViewModel.Id = (int)realmDto.Id!;
    realmDetailViewModel.Name = realmDto.Name!;
    realmDetailViewModel.Description = realmDto.Description;
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
      Image = realmDetailViewModel.Image,
      Resolution = realmDetailViewModel.Resolution,
      OriginX = realmDetailViewModel.OriginX,
      OriginY = realmDetailViewModel.OriginY,
      OriginRotation = realmDetailViewModel.OriginRotation
    };
  }

  public static RealmCreateDto ToCreateDto(this RealmDetailViewModel realmDetailViewModel)
  {
    return new RealmCreateDto {
      Name = realmDetailViewModel.Name,
      Description = realmDetailViewModel.Description,
      Image = realmDetailViewModel.Image,
      Resolution = realmDetailViewModel.Resolution,
      OriginX = realmDetailViewModel.OriginX,
      OriginY = realmDetailViewModel.OriginY,
      OriginRotation = realmDetailViewModel.OriginRotation
    };
  }
}