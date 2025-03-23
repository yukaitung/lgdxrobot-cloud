using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Identity;

public record TwoFactorRespondBusinessModel
{
  public required string SharedKey { get; set; }
  public required int RecoveryCodesLeft { get; set; }
  public IEnumerable<string>? RecoveryCodes { get; set; }
  public required bool IsTwoFactorEnabled { get; set; }
  public required bool IsMachineRemembered { get; set; }
}

public static class TwoFactorRespondBusinessModelExtensions
{
  public static TwoFactorRespondDto ToDto(this TwoFactorRespondBusinessModel twoFactorRespondBusinessModel)
  {
    return new TwoFactorRespondDto
    {
      SharedKey = twoFactorRespondBusinessModel.SharedKey,
      RecoveryCodesLeft = twoFactorRespondBusinessModel.RecoveryCodesLeft,
      RecoveryCodes = twoFactorRespondBusinessModel.RecoveryCodes,
      IsTwoFactorEnabled = twoFactorRespondBusinessModel.IsTwoFactorEnabled,
      IsMachineRemembered = twoFactorRespondBusinessModel.IsMachineRemembered
    };
  }
}