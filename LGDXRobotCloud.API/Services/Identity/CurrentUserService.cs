using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Identity;
using Microsoft.AspNetCore.Identity;

namespace LGDXRobotCloud.API.Services.Identity;

public interface ICurrentUserService
{
  Task<LgdxUserBusinessModel> GetUserAsync(string userId);
  Task<bool> UpdateUserAsync(string userId, LgdxUserUpdateBusinessModel lgdxUserBusinessModel);
  Task<TwoFactorRespondBusinessModel> UpdateTwoFactorAsync(string userId, TwoFactorRequestBusinessModel twoFactorRequestBusinessModel);
}

public class CurrentUserService(
  SignInManager<LgdxUser> signInManager,
    UserManager<LgdxUser> userManager
  ) : ICurrentUserService
{
  private readonly SignInManager<LgdxUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  public async Task<LgdxUserBusinessModel> GetUserAsync(string userId)
  {
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new LgdxNotFound404Exception();

    return new LgdxUserBusinessModel
    {
      Id = Guid.Parse(user.Id!),
      Name = user.Name ?? string.Empty,
      UserName = user.UserName ?? string.Empty,
      Email = user.Email ?? string.Empty,
      Roles = await _userManager.GetRolesAsync(user),
      TwoFactorEnabled = user.TwoFactorEnabled,
      AccessFailedCount = user.AccessFailedCount
    };
  }

  public async Task<bool> UpdateUserAsync(string userId, LgdxUserUpdateBusinessModel lgdxUserBusinessModel)
  {
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new LgdxNotFound404Exception();

    user.Name = lgdxUserBusinessModel.Name;
    user.Email = lgdxUserBusinessModel.Email;
    var result = await _userManager.UpdateAsync(user);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }

    return true;
  }

  public async Task<TwoFactorRespondBusinessModel> UpdateTwoFactorAsync(string userId, TwoFactorRequestBusinessModel twoFactorRequestBusinessModel)
  {
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new LgdxNotFound404Exception();

    if (twoFactorRequestBusinessModel.Enable == true)
    {
      if (twoFactorRequestBusinessModel.ResetSharedKey)
      {
        throw new LgdxValidation400Expection("CannotResetSharedKeyAndEnable",
          "Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.");
      }
      if (string.IsNullOrEmpty(twoFactorRequestBusinessModel.TwoFactorCode))
      {
        throw new LgdxValidation400Expection("RequiresTwoFactor",
          "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");
      }
      if (!await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactorRequestBusinessModel.TwoFactorCode))
      {
        throw new LgdxValidation400Expection("InvalidTwoFactorCode",
          "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");
      }

      await _userManager.SetTwoFactorEnabledAsync(user, true);
    }
    else if (twoFactorRequestBusinessModel.Enable == false || twoFactorRequestBusinessModel.ResetSharedKey)
    {
      await _userManager.SetTwoFactorEnabledAsync(user, false);
    }

    if (twoFactorRequestBusinessModel.ResetSharedKey)
    {
      await _userManager.ResetAuthenticatorKeyAsync(user);
    }

    string[]? recoveryCodes = null;
    if (twoFactorRequestBusinessModel.ResetRecoveryCodes || (twoFactorRequestBusinessModel.Enable == true && await _userManager.CountRecoveryCodesAsync(user) == 0))
    {
      var recoveryCodesEnumerable = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
      recoveryCodes = recoveryCodesEnumerable?.ToArray();
    }

    if (twoFactorRequestBusinessModel.ForgetMachine)
    {
      await _signInManager.ForgetTwoFactorClientAsync();
    }

    var key = await _userManager.GetAuthenticatorKeyAsync(user);
    if (string.IsNullOrEmpty(key))
    {
      await _userManager.ResetAuthenticatorKeyAsync(user);
      key = await _userManager.GetAuthenticatorKeyAsync(user);

      if (string.IsNullOrEmpty(key))
      {
        throw new NotSupportedException("The user manager must produce an authenticator key after reset.");
      }
    }

    return new TwoFactorRespondBusinessModel
    {
      SharedKey = key,
      RecoveryCodes = recoveryCodes,
      RecoveryCodesLeft = recoveryCodes?.Length ?? await _userManager.CountRecoveryCodesAsync(user),
      IsTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
      IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
    };
  }
}