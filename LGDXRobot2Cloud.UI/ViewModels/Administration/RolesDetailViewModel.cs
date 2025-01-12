using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Administration;

public sealed class RolesDetailViewModel : FormViewModel, IValidatableObject
{
  public Guid Id { get; set; }
  
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  public string? Description { get; set; }

  public List<string> Scopes { get; set; } = [];

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    int i = 0;
    foreach (var scope in Scopes)
    {
      if (string.IsNullOrWhiteSpace(scope))
      {
        yield return new ValidationResult($"Scope is required.", [$"{nameof(Scopes)}-{i}"]);
      }
      i++;
    }
  }
}

public static class RolesDetailViewModelExtensions
{
  public static void FromDto(this RolesDetailViewModel roleDetailViewModel, LgdxRoleDto lgdxRoleDto)
  {
    roleDetailViewModel.Id = (Guid)lgdxRoleDto.Id!;
    roleDetailViewModel.Name = lgdxRoleDto.Name!;
    roleDetailViewModel.Description = lgdxRoleDto.Description;
    roleDetailViewModel.Scopes = lgdxRoleDto.Scopes!;
  }

  public static LgdxRoleUpdateDto ToUpdateDto(this RolesDetailViewModel roleDetailViewModel)
  {
    return new LgdxRoleUpdateDto {
      Name = roleDetailViewModel.Name,
      Description = roleDetailViewModel.Description,
      Scopes = roleDetailViewModel.Scopes
    };
  }

  public static LgdxRoleCreateDto ToCreateDto(this RolesDetailViewModel roleDetailViewModel)
  {
    return new LgdxRoleCreateDto {
      Name = roleDetailViewModel.Name,
      Description = roleDetailViewModel.Description,
      Scopes = roleDetailViewModel.Scopes
    };
  }
}