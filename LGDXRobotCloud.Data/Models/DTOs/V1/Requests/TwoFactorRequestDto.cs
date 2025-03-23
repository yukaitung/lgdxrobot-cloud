using LGDXRobotCloud.Data.Models.Business.Identity;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Requests;

public record TwoFactorRequestDto
{
  public bool Enable { get; set; } = false;
  public string TwoFactorCode { get; set; } = string.Empty;
  public bool ResetRecoveryCodes { get; set; } = false;
  public bool ResetSharedKey { get; set; } = false;
  public bool ForgetMachine { get; set; } = false;
}

public static class TwoFactorRequestDtoExtensions
{
  public static TwoFactorRequestBusinessModel ToBusinessModel(this TwoFactorRequestDto twoFactorRequestDto)
  {
    return new TwoFactorRequestBusinessModel
    {
      Enable = twoFactorRequestDto.Enable,
      TwoFactorCode = twoFactorRequestDto.TwoFactorCode,
      ResetRecoveryCodes = twoFactorRequestDto.ResetRecoveryCodes,
      ResetSharedKey = twoFactorRequestDto.ResetSharedKey,
      ForgetMachine = twoFactorRequestDto.ForgetMachine
    };
  }
}