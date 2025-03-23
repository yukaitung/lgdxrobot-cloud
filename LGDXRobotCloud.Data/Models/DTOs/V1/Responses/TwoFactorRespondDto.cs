namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record TwoFactorRespondDto
{
  public required string SharedKey { get; set; }
  public required int RecoveryCodesLeft { get; set; }
  public IEnumerable<string>? RecoveryCodes { get; set; }
  public required bool IsTwoFactorEnabled { get; set; }
  public required bool IsMachineRemembered { get; set; }
}