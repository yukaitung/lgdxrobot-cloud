using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Identity;

public class TwoFactorViewModel : FormViewModelBase, IValidatableObject
{
  private const int _twoFactorCodeLength = 6;

  public List<string?> TwoFactorCode { get; set; } = [];

  public string? TwoFactorRecoveryCode { get; set; } = null!;

  public void SetupTwoFactor()
  {
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
      for (int i = 0; i < TwoFactorCode.Count; i++)
      {
        bool isValid = true;
        if (string.IsNullOrWhiteSpace(TwoFactorCode[i]))
        {
          isValid = false;
        }
        if (!isValid)
        {
          yield return new ValidationResult("Please enter a code.", [nameof(TwoFactorCode)]);
        }
      }
    }
  }
}