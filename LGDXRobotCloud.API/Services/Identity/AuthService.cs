using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Identity;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LGDXRobotCloud.API.Services.Identity;

public interface IAuthService
{
  Task<LoginResponseBusinessModel> LoginAsync(LoginRequestBusinessModel loginRequestBusinessModel);
  Task ForgotPasswordAsync(ForgotPasswordRequestBusinessModel forgotPasswordRequestBusinessModel);
  Task ResetPasswordAsync(ResetPasswordRequestBusinessModel resetPasswordRequestBusinessModel);
  Task<RefreshTokenResponseBusinessModel> RefreshTokenAsync(RefreshTokenRequestBusinessModel refreshTokenRequestBusinessModel);
  Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequestBusinessModel updatePasswordRequestBusinessModel);
}

public class AuthService(
    IActivityLogService activityLogService,
    LgdxContext context,
    IEmailService emailService,
    IOptionsSnapshot<LgdxRobotCloudSecretConfiguration> lgdxRobotCloudSecretConfiguration,
    SignInManager<LgdxUser> signInManager,
    UserManager<LgdxUser> userManager
  ) : IAuthService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly LgdxRobotCloudSecretConfiguration _lgdxRobotCloudSecretConfiguration = lgdxRobotCloudSecretConfiguration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobotCloudSecretConfiguration));
  private readonly SignInManager<LgdxUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  private JwtSecurityToken GenerateJwtToken(List<Claim> claims, DateTime notBefore, DateTime expires)
  {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_lgdxRobotCloudSecretConfiguration.LgdxUserJwtSecret));
    var credentials = new SigningCredentials(securityKey, _lgdxRobotCloudSecretConfiguration.LgdxUserJwtAlgorithm);
    return new JwtSecurityToken(
      _lgdxRobotCloudSecretConfiguration.LgdxUserJwtIssuer,
      _lgdxRobotCloudSecretConfiguration.LgdxUserJwtIssuer,
      claims,
      notBefore,
      expires,
      credentials);
  }

  private async Task<string> GenerateAccessTokenAsync(LgdxUser user, DateTime notBefore, DateTime expires)
  {
    var userRoles = await _userManager.GetRolesAsync(user);
    var Claims = new List<Claim>
    {
      new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new (ClaimTypes.Name, user.UserName ?? string.Empty),
      new (ClaimTypes.Email, user.Email ?? string.Empty),
      new ("fullname", user.Name ?? string.Empty),
    };
    // Add Roles         
    foreach (var userRole in userRoles)
    {
      Claims.Add(new Claim(ClaimTypes.Role, userRole));
    }
    // Add Role Claims
    {
      List<string> roleIds = await _context.Roles.AsNoTracking()
        .Where(r => userRoles.Select(ur => ur.ToUpper()).Contains(r.NormalizedName!))
        .Select(r => r.Id )
        .ToListAsync();
      var roleClaims = await _context.RoleClaims.AsNoTracking()
        .Where(r => roleIds.Contains(r.RoleId))
        .ToListAsync();
      foreach (var roleClaim in roleClaims)
      {
        Claims.Add(new Claim(roleClaim.ClaimType!, roleClaim.ClaimValue!));
      }
    }
    var token = GenerateJwtToken(Claims, notBefore, expires);
    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  private string GenerateRefreshToken(LgdxUser user, DateTime notBefore, DateTime expires)
  {
    var Claims = new List<Claim>
    {
      new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    };
    var token = GenerateJwtToken(Claims, notBefore, expires);
    var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
    return tokenStr;
  }

  public async Task<LoginResponseBusinessModel> LoginAsync(LoginRequestBusinessModel loginRequestBusinessModel)
  {
    var user = await _userManager.FindByNameAsync(loginRequestBusinessModel.Username) 
      ?? throw new LgdxValidation400Expection(nameof(loginRequestBusinessModel.Username), "The username or password is invalid.");

    var loginResult = await _signInManager.PasswordSignInAsync(user, loginRequestBusinessModel.Password, false, true);
    if (loginResult.RequiresTwoFactor)
    {
      if (!string.IsNullOrEmpty(loginRequestBusinessModel.TwoFactorCode))
      {
        loginResult = await _signInManager.TwoFactorAuthenticatorSignInAsync(loginRequestBusinessModel.TwoFactorCode, false, false);
        if (!loginResult.Succeeded)
        {
          throw new LgdxValidation400Expection(nameof(loginRequestBusinessModel.Username), "The 2FA code is invalid.");
        }
      }
      else if (!string.IsNullOrEmpty(loginRequestBusinessModel.TwoFactorRecoveryCode))
      {
        loginResult = await _signInManager.TwoFactorRecoveryCodeSignInAsync(loginRequestBusinessModel.TwoFactorRecoveryCode);
        if (!loginResult.Succeeded)
        {
          throw new LgdxValidation400Expection(nameof(loginRequestBusinessModel.Username), "The recovery code is invalid.");
        }
      }
      else
      {
        return new LoginResponseBusinessModel 
        {
          AccessToken = string.Empty,
          RefreshToken = string.Empty,
          ExpiresMins = 0,
          RequiresTwoFactor = true
        };
      }
    }
    if (!loginResult.Succeeded)
    {
      if (loginResult.IsLockedOut)
      {
        await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
        {
          EntityName = nameof(LgdxUser),
          EntityId = user.Id.ToString(),
          Action = (int)ActivityAction.LoginFailed,
          Note = "The account is locked out."
        });
        throw new LgdxValidation400Expection(nameof(loginRequestBusinessModel.Username), "The account is locked out.");
      }
      else
      {
        await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
        {
          EntityName = nameof(LgdxUser),
          EntityId = user.Id.ToString(),
          Action = (int)ActivityAction.LoginFailed,
          Note = "The username or password is invalid."
        });
        throw new LgdxValidation400Expection(nameof(loginRequestBusinessModel.Username), "The username or password is invalid.");
      }
    }

    var notBefore = DateTime.UtcNow;
    var accessExpires = notBefore.AddMinutes(_lgdxRobotCloudSecretConfiguration.LgdxUserAccessTokenExpiresMins);
    var refreshExpires = notBefore.AddMinutes(_lgdxRobotCloudSecretConfiguration.LgdxUserRefreshTokenExpiresMins);
    var accessToken = await GenerateAccessTokenAsync(user, notBefore, accessExpires);
    var refreshToken = GenerateRefreshToken(user, notBefore, refreshExpires);
    user.RefreshTokenHash = LgdxHelper.GenerateSha256Hash(refreshToken);
    var updateTokenResult = await _userManager.UpdateAsync(user);
    if (!updateTokenResult.Succeeded)
    {
      throw new LgdxIdentity400Expection(updateTokenResult.Errors);
    }
    await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id.ToString(),
      Action = (int)ActivityAction.LoginSuccess,
    });
    return new LoginResponseBusinessModel
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      ExpiresMins = _lgdxRobotCloudSecretConfiguration.LgdxUserAccessTokenExpiresMins,
      RequiresTwoFactor = false
    };
  }

  public async Task ForgotPasswordAsync(ForgotPasswordRequestBusinessModel forgotPasswordRequestBusinessModel)
  {
    var user = await _userManager.FindByEmailAsync(forgotPasswordRequestBusinessModel.Email);
    if (user != null)
    {
      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      await _emailService.SendPasswordResetEmailAsync(user.Email!, user.Name!, user.UserName!, token);
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(LgdxUser),
        EntityId = user.Id.ToString(),
        Action = (int)ActivityAction.UserPasswordReset,
      });
    }
    // For security reasons, we do not return a 404 status code.
  }

  public async Task ResetPasswordAsync(ResetPasswordRequestBusinessModel resetPasswordRequestBusinessModel)
  {
    var user = await _userManager.FindByEmailAsync(resetPasswordRequestBusinessModel.Email)
      ?? throw new LgdxValidation400Expection(nameof(resetPasswordRequestBusinessModel.Token), "");

    var result = await _userManager.ResetPasswordAsync(user, resetPasswordRequestBusinessModel.Token, resetPasswordRequestBusinessModel.NewPassword);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }
    await _emailService.SendPasswordUpdateEmailAsync(user.Email!, user.Name!, user.UserName!);
    await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id.ToString(),
      Action = (int)ActivityAction.UserPasswordUpdated,
    });
  }

  public async Task<RefreshTokenResponseBusinessModel> RefreshTokenAsync(RefreshTokenRequestBusinessModel refreshTokenRequestBusinessModel)
  {
    // Validate the refresh token
    var tokenHandler = new JwtSecurityTokenHandler();
    TokenValidationParameters validationParameters = new()
    {
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = _lgdxRobotCloudSecretConfiguration.LgdxUserJwtIssuer,
			ValidAudience = _lgdxRobotCloudSecretConfiguration.LgdxUserJwtIssuer,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_lgdxRobotCloudSecretConfiguration.LgdxUserJwtSecret)),
			ClockSkew = TimeSpan.Zero
		};
    ClaimsPrincipal principal = tokenHandler.ValidateToken(refreshTokenRequestBusinessModel.RefreshToken, validationParameters, out SecurityToken validatedToken);

    // The token is valid, check the database
    var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
      ?? throw new LgdxValidation400Expection(nameof(refreshTokenRequestBusinessModel.RefreshToken), "The user ID is not found in the token.");
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new LgdxValidation400Expection(nameof(refreshTokenRequestBusinessModel.RefreshToken), "User not found.");
    if (user.RefreshTokenHash != LgdxHelper.GenerateSha256Hash(refreshTokenRequestBusinessModel.RefreshToken))
    {
      throw new LgdxValidation400Expection(nameof(refreshTokenRequestBusinessModel.RefreshToken), "The refresh token is used.");
    }

    // Generate new token pair and update the database
    var notBefore = DateTime.UtcNow;
    var accessExpires = notBefore.AddMinutes(_lgdxRobotCloudSecretConfiguration.LgdxUserAccessTokenExpiresMins);
    var refreshExpires = validatedToken.ValidTo; // Reauthenticate to extend the expiration time
    var accessToken = await GenerateAccessTokenAsync(user, notBefore, accessExpires);
    var refreshToken = GenerateRefreshToken(user, notBefore, refreshExpires);
    user.RefreshTokenHash = LgdxHelper.GenerateSha256Hash(refreshToken);
    var result = await _userManager.UpdateAsync(user);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }

    // Generate new token pair
    return new RefreshTokenResponseBusinessModel()
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      ExpiresMins = _lgdxRobotCloudSecretConfiguration.LgdxUserAccessTokenExpiresMins
    };
  }

  public async Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequestBusinessModel updatePasswordRequestBusinessModel)
  {
    var user = await userManager.FindByIdAsync(userId)
      ?? throw new LgdxNotFound404Exception();

    var result = await _userManager.ChangePasswordAsync(user, updatePasswordRequestBusinessModel.CurrentPassword, updatePasswordRequestBusinessModel.NewPassword);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }
    await _emailService.SendPasswordUpdateEmailAsync(user.Email!, user.Name!, user.UserName!);
    await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(LgdxUser),
      EntityId = user.Id.ToString(),
      Action = (int)ActivityAction.UserPasswordUpdated,
    });
    return true;
  }
}