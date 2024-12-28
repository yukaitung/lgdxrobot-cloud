using System.ComponentModel.DataAnnotations;
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

  [Required]
  public double Resolution { get; set; }

  [Required]
  public double OriginX { get; set; }

  [Required]
  public double OriginY { get; set; }

  [Required]
  public double OriginRotation { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Id == null && SelectedImage == null)
    {
      yield return new ValidationResult("Please select an image.", [nameof(SelectedImage)]);
    }
  }
}