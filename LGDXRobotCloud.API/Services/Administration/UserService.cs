using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.API.Services.Administration;

public interface IUserService
{
  Task<(IEnumerable<LgdxUserListBusinessModel>, PaginationHelper)> GetUsersAsync(string? name, int pageNumber, int pageSize);
  Task<LgdxUserBusinessModel> GetUserAsync(Guid id);
  Task<LgdxUserBusinessModel> CreateUserAsync(LgdxUserCreateAdminBusinessModel lgdxUserCreateAdminBusinessModel);
  Task<bool> UpdateUserAsync(Guid id, LgdxUserUpdateAdminBusinessModel lgdxUserUpdateAdminBusinessModel);
  Task<bool> UnlockUserAsync(Guid id);
  Task<bool> DeleteUserAsync(Guid id, string operatorId);
}

public class UserService(
    IActivityLogService activityLogService,
    IEmailService emailService,
    LgdxContext context,
    UserManager<LgdxUser> userManager
  ) : IUserService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  public async Task<(IEnumerable<LgdxUserListBusinessModel>, PaginationHelper)> GetUsersAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Users as IQueryable<LgdxUser>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim().ToUpper();
      query = query.Where(u => u.NormalizedUserName!.Contains(name.ToUpper()));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var users = await query.AsNoTracking()
      .OrderBy(t => t.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(t => new LgdxUserListBusinessModel {
        Id = Guid.Parse(t.Id!),
        Name = t.Name!,
        UserName = t.UserName!,
        TwoFactorEnabled = t.TwoFactorEnabled,
        AccessFailedCount = t.AccessFailedCount,
      })
      .ToListAsync();
    return (users, PaginationHelper);
  }

  public async Task<LgdxUserBusinessModel> GetUserAsync(Guid id)
  {
    var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new LgdxNotFound404Exception();
    var roles = await _userManager.GetRolesAsync(user);
    return new LgdxUserBusinessModel {
      Id = Guid.Parse(user.Id),
      Name = user.Name!,
      UserName = user.UserName!,
      Email = user.Email!,
      Roles = roles,
      TwoFactorEnabled = user.TwoFactorEnabled,
      AccessFailedCount = user.AccessFailedCount,
      LockoutEnd = user.LockoutEnd?.DateTime,
    };
  }

  public async Task<LgdxUserBusinessModel> CreateUserAsync(LgdxUserCreateAdminBusinessModel lgdxUserCreateAdminBusinessModel)
  {
    var user = new LgdxUser {
      Id = Guid.CreateVersion7().ToString(),
      Email = lgdxUserCreateAdminBusinessModel.Email,
      EmailConfirmed = true,
      LockoutEnabled = true,
      Name = lgdxUserCreateAdminBusinessModel.Name,
      NormalizedEmail = lgdxUserCreateAdminBusinessModel.Email.ToUpper(),
      NormalizedUserName = lgdxUserCreateAdminBusinessModel.UserName.ToUpper(),
      SecurityStamp = Guid.CreateVersion7().ToString(),
      UserName = lgdxUserCreateAdminBusinessModel.UserName
    };
    if (!string.IsNullOrWhiteSpace(lgdxUserCreateAdminBusinessModel.Password))
    {
      var result = await _userManager.CreateAsync(user, lgdxUserCreateAdminBusinessModel.Password);
      if (!result.Succeeded)
      {
        throw new LgdxIdentity400Expection(result.Errors);
      }
    }
    else
    {
      var result = await _userManager.CreateAsync(user);
      if (!result.Succeeded)
      {
        throw new LgdxIdentity400Expection(result.Errors);
      }
    }

    // Add Roles
    var roleToAdd = lgdxUserCreateAdminBusinessModel.Roles;
    var roleAddingResult = await _userManager.AddToRolesAsync(user, roleToAdd);
    if (!roleAddingResult.Succeeded)
    {
      throw new LgdxIdentity400Expection(roleAddingResult.Errors);
    }

    // Send Email
    if (string.IsNullOrWhiteSpace(lgdxUserCreateAdminBusinessModel.Password))
    {
      // No password is specified
      var token = await _userManager.GeneratePasswordResetTokenAsync(user!);
      await _emailService.SendWellcomePasswordSetEmailAsync(
        lgdxUserCreateAdminBusinessModel.Email, 
        lgdxUserCreateAdminBusinessModel.Name, 
        lgdxUserCreateAdminBusinessModel.UserName,
        token
      );
    }
    else
    {
      // Password is specified
      await _emailService.SendWelcomeEmailAsync(
        lgdxUserCreateAdminBusinessModel.Email, 
        lgdxUserCreateAdminBusinessModel.Name, 
        lgdxUserCreateAdminBusinessModel.UserName
      );
    }
    
    await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id,
      Action = ActivityAction.Create,
    });

    return new LgdxUserBusinessModel
    {
      Id = Guid.Parse(user.Id),
      Name = user.Name!,
      UserName = user.UserName!,
      Email = user.Email!,
      Roles = lgdxUserCreateAdminBusinessModel.Roles,
      TwoFactorEnabled = user.TwoFactorEnabled,
      AccessFailedCount = user.AccessFailedCount,
    };
  }

  public async Task<bool> UpdateUserAsync(Guid id, LgdxUserUpdateAdminBusinessModel lgdxUserUpdateAdminBusinessModel)
  {
    var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new LgdxNotFound404Exception();
    
    user.Name = lgdxUserUpdateAdminBusinessModel.Name;
    user.UserName = lgdxUserUpdateAdminBusinessModel.UserName;
    user.Email = lgdxUserUpdateAdminBusinessModel.Email;
    user.NormalizedEmail = lgdxUserUpdateAdminBusinessModel.Email.ToUpper();
    user.NormalizedUserName = lgdxUserUpdateAdminBusinessModel.UserName.ToUpper();

    var result = await _userManager.UpdateAsync(user);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }

    var roles = await _userManager.GetRolesAsync(user);
    var roleToAdd = lgdxUserUpdateAdminBusinessModel.Roles.Except(roles);
    result = await _userManager.AddToRolesAsync(user, roleToAdd);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }
    var roleToRemove = roles.Except(lgdxUserUpdateAdminBusinessModel.Roles);
    result = await _userManager.RemoveFromRolesAsync(user, roleToRemove);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }

    await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id,
      Action = ActivityAction.Update,
    });

    return true;
  }

  public async Task<bool> UnlockUserAsync(Guid id)
  {
    var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new LgdxNotFound404Exception();

    user.AccessFailedCount = 0;
    user.LockoutEnd = null;

    var result = await _userManager.UpdateAsync(user);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }

    await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id,
      Action = ActivityAction.UserUnlocked,
    });

    return true;
  }

  public async Task<bool> DeleteUserAsync(Guid id, string operatorId)
  {
    var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new LgdxNotFound404Exception();
    if (user.Id == operatorId)
    {
      throw new LgdxValidation400Expection(nameof(id), "Cannot delete yourself.");
    }
    var result = await _userManager.DeleteAsync(user);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }

    await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id,
      Action = ActivityAction.Delete,
    });
    
    return true;
  }
}