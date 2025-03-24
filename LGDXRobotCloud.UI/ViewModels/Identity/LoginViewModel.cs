using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Identity;

public sealed class LoginViewModel : FormViewModel, IValidatableObject
{
  private const int _twoFactorCodeLength = 6;

  [Required (ErrorMessage = "Please enter a username.")]
  public string Username { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a password.")]
  public string Password { get; set; } = null!;

  public bool RequiresTwoFactor { get; set; } = false;

  public List<string?> TwoFactorCode { get; set; } = [];

  public string? TwoFactorRecoveryCode { get; set; } = null!;

  public void SetupTwoFactor()
  {
    RequiresTwoFactor = true;
    TwoFactorCode = [];
    for (int i = 0; i < _twoFactorCodeLength; i++)
    {
      TwoFactorCode.Add(null);
    }
  }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (TwoFactorCode != null)
    {
      bool isValid = true;
      for (int i = 0; i < TwoFactorCode.Count; i++)
      {
        if (string.IsNullOrWhiteSpace(TwoFactorCode[i]))
        {
          isValid = false;
        }
      }
      if (!isValid)
      {
        yield return new ValidationResult("Please enter a code.", [nameof(TwoFactorCode)]);
      }
    }
  }
}

public static class LoginViewModelExtensions
{
  public static LoginRequestDto ToLoginRequestDto(this LoginViewModel loginViewModel, string? twoFactorCode, string? twoFactorRecoveryCode)
  {
    return new LoginRequestDto {
      Username = loginViewModel.Username,
      Password = loginViewModel.Password,
      TwoFactorCode = twoFactorCode,
      TwoFactorRecoveryCode = twoFactorRecoveryCode
    };
  }
}