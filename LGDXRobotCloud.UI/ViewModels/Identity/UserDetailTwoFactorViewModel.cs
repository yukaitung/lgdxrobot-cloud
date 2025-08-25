using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Identity;

public class UserDetailTwoFactorViewModel : FormViewModel
{
  [Required (ErrorMessage = "Please enter a verification code.")]
  public string VerficationCode { get; set; } = null!;

  public string SvgGraphicsPath { get; set; } = string.Empty;

  public string SharedKey { get; set; } = string.Empty;

  public List<string> RecoveryCodes { get; set; } = [];

  public int Step { get; set; } = 0;
}