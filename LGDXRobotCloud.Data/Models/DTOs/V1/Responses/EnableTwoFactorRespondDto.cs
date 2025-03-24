namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public class EnableTwoFactorRespondDto
{
  public required IEnumerable<string> RecoveryCodes { get; set; }
}