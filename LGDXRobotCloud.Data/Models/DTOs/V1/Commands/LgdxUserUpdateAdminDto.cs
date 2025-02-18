using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Administration;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record LgdxUserUpdateAdminDto : IValidatableObject
{
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  [Required (ErrorMessage = "Please enter a username.")]
  public required string UserName { get; set; }

  [Required (ErrorMessage = "Please enter an email.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public required string Email { get; set; }

  public IEnumerable<string> Roles { get; set; } = [];

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (!Roles.Any())
    {
      yield return new ValidationResult("At least one role is required.", [nameof(Roles)]);
    }
    int i = 0;
    foreach (var role in Roles)
    {
      if (string.IsNullOrWhiteSpace(role))
      {
        yield return new ValidationResult($"Role is required.", [nameof(Roles), $"{nameof(Roles)}-{i}"]);
      }
      i++;
    }
  }
}

public static class LgdxUserUpdateAdminDtoExtensions
{
  public static LgdxUserUpdateAdminBusinessModel ToBusinessModel(this LgdxUserUpdateAdminDto model)
  {
    return new LgdxUserUpdateAdminBusinessModel {
      Name = model.Name,
      UserName = model.UserName,
      Email = model.Email,
      Roles = model.Roles,
    };
  }
}