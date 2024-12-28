using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record LgdxUserUpdateDto
{
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  [Required (ErrorMessage = "Please enter an email.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public required string Email { get; set; }
}