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
  Task<string> InitiateTwoFactorAsync(string userId);
  Task<List<string>> EnableTwoFactorAsync(string userId, string twoFactorCode);
  Task<List<string>> ResetRecoveryCodesAsync(string userId);
  Task<bool> DisableTwoFactorAsync(string userId);
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

  public async Task<string> InitiateTwoFactorAsync(string userId)
  {
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new LgdxNotFound404Exception();

    await _userManager.SetTwoFactorEnabledAsync(user, false);   
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
    return key;
  }

  public async Task<List<string>> EnableTwoFactorAsync(string userId, string twoFactorCode)
  {
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new LgdxNotFound404Exception();

    if (string.IsNullOrEmpty(twoFactorCode))
    {
      throw new LgdxValidation400Expection("RequiresTwoFactor",
        "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");
    }
    if (!await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactorCode))
      {
        throw new LgdxValidation400Expection("InvalidTwoFactorCode",
          "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");
      }
    await _userManager.SetTwoFactorEnabledAsync(user, true);

    await _userManager.CountRecoveryCodesAsync(user);
    var recoveryCodesEnumerable = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
    return recoveryCodesEnumerable?.ToList() ?? [];
  }

  public async Task<List<string>> ResetRecoveryCodesAsync(string userId)
  {
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new LgdxNotFound404Exception();
    
    var recoveryCodesEnumerable = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
    return recoveryCodesEnumerable?.ToList() ?? [];
  }

  public async Task<bool> DisableTwoFactorAsync(string userId)
  { 
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new LgdxNotFound404Exception();

    await _userManager.ResetAuthenticatorKeyAsync(user);
    return true;
  }
}