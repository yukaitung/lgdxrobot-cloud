namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public class ResetRecoveryCodesRespondDto
{
  public required IEnumerable<string> RecoveryCodes { get; set; }
}