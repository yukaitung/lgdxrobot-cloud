namespace LGDXRobotCloud.Data.Models.Business.Identity;

public record TwoFactorRequestBusinessModel
{
  public bool Enable { get; set; } = false;
  public string TwoFactorCode { get; set; } = string.Empty;
  public bool ResetRecoveryCodes { get; set; } = false;
  public bool ResetSharedKey { get; set; } = false;
  public bool ForgetMachine { get; set; } = false;
}