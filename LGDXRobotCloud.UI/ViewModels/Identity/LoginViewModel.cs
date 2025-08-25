using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Identity;

public enum LoginViewModelState
{
  Username = 0,
  TwoFactorCode = 1,
  TwoFactorRecoveryCode = 2,
}

public class LoginViewModel : FormViewModel, IValidatableObject
{
  private const int _twoFactorCodeLength = 6;

  public LoginViewModelState State { get; set; } = LoginViewModelState.Username;

  [Required (ErrorMessage = "Please enter a username.")]
  public string Username { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a password.")]
  public string Password { get; set; } = null!;

  public List<string?> TwoFactorCode { get; set; } = [];

  public bool InputRecoveryCode { get; set; } = false;

  public string? TwoFactorRecoveryCode { get; set; } = null!;

  public string? TimeZone { get; set; }

  public void SetupTwoFactor()
  {
    State = LoginViewModelState.TwoFactorCode;
    TwoFactorCode = [];
    for (int i = 0; i < _twoFactorCodeLength; i++)
    {
      TwoFactorCode.Add(null);
    }
  }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (InputRecoveryCode == false && State == LoginViewModelState.TwoFactorCode)
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
    if (State == LoginViewModelState.TwoFactorRecoveryCode && string.IsNullOrWhiteSpace(TwoFactorRecoveryCode))
    {
      yield return new ValidationResult("Please enter a recovery code.", [nameof(TwoFactorRecoveryCode)]);
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