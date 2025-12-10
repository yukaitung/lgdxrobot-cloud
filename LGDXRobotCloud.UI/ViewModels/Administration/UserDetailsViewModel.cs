using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Administration;

public class UserDetailsViewModel : FormViewModelBase, IValidatableObject
{
  public Guid Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a username.")]
  public string UserName { get; set; } = null!;

  [Required (ErrorMessage = "Please enter an email.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public string Email { get; set; } = null!;

  public string Password { get; set; } = null!;

  public List<string> Roles { get; set; } = [];

  public bool TwoFactorEnabled { get; set; }
  
  public int AccessFailedCount { get; set; }

  public DateTimeOffset? LockoutEnd { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Roles.Count == 0)
    {
      yield return new ValidationResult("At least one role is required.", [nameof(Roles)]);
    }
    int i = 0;
    foreach (var role in Roles)
    {
      if (string.IsNullOrWhiteSpace(role))
      {
        yield return new ValidationResult($"Role is required.", [$"{nameof(Roles)}-{i}"]);
      }
      i++;
    }
  }
}

public static class UserDetailsViewModelExtensions
{
  public static void FromDto(this UserDetailsViewModel UserDetailsViewModel, LgdxUserDto lgdxUserDto)
  {
    UserDetailsViewModel.Id = (Guid)lgdxUserDto.Id!;
    UserDetailsViewModel.Name = lgdxUserDto.Name!;
    UserDetailsViewModel.UserName = lgdxUserDto.UserName!;
    UserDetailsViewModel.Email = lgdxUserDto.Email!;
    UserDetailsViewModel.Roles = lgdxUserDto.Roles!;
    UserDetailsViewModel.TwoFactorEnabled = (bool)lgdxUserDto.TwoFactorEnabled!;
    UserDetailsViewModel.AccessFailedCount = (int)lgdxUserDto.AccessFailedCount!;
    UserDetailsViewModel.LockoutEnd = lgdxUserDto.LockoutEnd;
  }

  public static LgdxUserUpdateAdminDto ToUpdateDto(this UserDetailsViewModel UserDetailsViewModel)
  {
    return new LgdxUserUpdateAdminDto {
      Name = UserDetailsViewModel.Name,
      UserName = UserDetailsViewModel.UserName,
      Email = UserDetailsViewModel.Email,
      Roles = UserDetailsViewModel.Roles
    };
  }

  public static LgdxUserCreateAdminDto ToCreateDto(this UserDetailsViewModel UserDetailsViewModel)
  {
    return new LgdxUserCreateAdminDto {
      Name = UserDetailsViewModel.Name,
      UserName = UserDetailsViewModel.UserName,
      Email = UserDetailsViewModel.Email,
      Roles = UserDetailsViewModel.Roles,
      Password = UserDetailsViewModel.Password
    };
  }
}