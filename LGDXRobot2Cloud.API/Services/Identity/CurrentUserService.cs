using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Business.Administration;
using LGDXRobot2Cloud.Data.Models.Business.Identity;
using Microsoft.AspNetCore.Identity;

namespace LGDXRobot2Cloud.API.Services.Identity;

public interface ICurrentUserService
{
  Task<LgdxUserBusinessModel> GetUserAsync(string userId);
  Task<bool> UpdateUserAsync(string userId, LgdxUserUpdateBusinessModel lgdxUserBusinessModel);
}

public class CurrentUserService(
    UserManager<LgdxUser> userManager
  ) : ICurrentUserService
{
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  public async Task<LgdxUserBusinessModel> GetUserAsync(string userId)
  {
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new LgdxNotFound404Exception();

    return new LgdxUserBusinessModel {
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
}