using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Identity;
using LGDXRobotCloud.Utilities.Enums;
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
    IActivityLogService activityLogService,
    UserManager<LgdxUser> userManager
  ) : ICurrentUserService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
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

    await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id.ToString(),
      Action = ActivityAction.Update,
    });

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
      throw new LgdxValidation400Expection("RequiresTwoFactor", "The 2FA code is required.");
    }
    if (!await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactorCode))
      {
        throw new LgdxValidation400Expection("InvalidTwoFactorCode", "The 2FA code is invalid.");
      }
    await _userManager.SetTwoFactorEnabledAsync(user, true);

    await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id.ToString(),
      Action = ActivityAction.UserTwoFactorAuthenticationEnabled,
    });

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
    await _userManager.SetTwoFactorEnabledAsync(user, false);

    await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id.ToString(),
      Action = ActivityAction.UserTwoFactorAuthenticationDisabled,
    });
    return true;
  }
}