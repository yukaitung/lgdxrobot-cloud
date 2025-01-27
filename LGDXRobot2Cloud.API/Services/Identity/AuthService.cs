using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.API.Services.Common;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Business.Identity;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LGDXRobot2Cloud.API.Services.Identity;

public interface IAuthService
{
  Task<LoginResponseBusinessModel> LoginAsync(LoginRequestBusinessModel loginRequestBusinessModel);
  Task ForgotPasswordAsync(ForgotPasswordRequestBusinessModel forgotPasswordRequestBusinessModel);
  Task ResetPasswordAsync(ResetPasswordRequestBusinessModel resetPasswordRequestBusinessModel);
  Task<RefreshTokenResponseBusinessModel> RefreshTokenAsync(RefreshTokenRequestBusinessModel refreshTokenRequestBusinessModel);
  Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequestBusinessModel updatePasswordRequestBusinessModel);
}

public class AuthService(
    LgdxContext context,
    IEmailService emailService,
    IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
    UserManager<LgdxUser> userManager
  ) : IAuthService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobot2SecretConfiguration));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  private JwtSecurityToken GenerateJwtToken(List<Claim> claims, DateTime notBefore, DateTime expires)
  {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_lgdxRobot2SecretConfiguration.LgdxUserJwtSecret));
    var credentials = new SigningCredentials(securityKey, _lgdxRobot2SecretConfiguration.LgdxUserJwtAlgorithm);
    return new JwtSecurityToken(
      _lgdxRobot2SecretConfiguration.LgdxUserJwtIssuer,
      _lgdxRobot2SecretConfiguration.LgdxUserJwtIssuer,
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
      ?? throw new LgdxValidation400Expection(nameof(loginRequestBusinessModel.Username), "The user does not exist.");

    if (await _userManager.IsLockedOutAsync(user))
    {
      throw new LgdxValidation400Expection(nameof(loginRequestBusinessModel.Username), "The user is locked out.");
    }

    // Check password and generate token
    if (await _userManager.CheckPasswordAsync(user, loginRequestBusinessModel.Password))
    {
      var notBefore = DateTime.UtcNow;
      var accessExpires = notBefore.AddMinutes(_lgdxRobot2SecretConfiguration.LgdxUserAccessTokenExpiresMins);
      var refreshExpires = notBefore.AddMinutes(_lgdxRobot2SecretConfiguration.LgdxUserRefreshTokenExpiresMins);
      var accessToken = await GenerateAccessTokenAsync(user, notBefore, accessExpires);
      var refreshToken = GenerateRefreshToken(user, notBefore, refreshExpires);
      user.RefreshTokenHash = LgdxHelper.GenerateSha256Hash(refreshToken);
      var result = await _userManager.UpdateAsync(user);
      if (!result.Succeeded)
      {
        throw new LgdxIdentity400Expection(result.Errors);
      }
      return new LoginResponseBusinessModel 
      {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresMins = _lgdxRobot2SecretConfiguration.LgdxUserAccessTokenExpiresMins
      };
    }
    else
    {
      // Password is incorrect
      var incrementLockoutResult = await _userManager.AccessFailedAsync(user);
      if (!incrementLockoutResult.Succeeded)
      {
        // Return the same failure we do when resetting the lockout fails after a correct password.
        throw new LgdxIdentity400Expection(incrementLockoutResult.Errors);
      }
      if (await _userManager.IsLockedOutAsync(user))
      {
        throw new LgdxValidation400Expection(nameof(loginRequestBusinessModel.Username), "The user is locked out.");
      }
    }    
    throw new LgdxValidation400Expection(nameof(loginRequestBusinessModel.Username), "Login failed.");
  }

  public async Task ForgotPasswordAsync(ForgotPasswordRequestBusinessModel forgotPasswordRequestBusinessModel)
  {
    var user = await _userManager.FindByEmailAsync(forgotPasswordRequestBusinessModel.Email);
    if (user != null)
    {
      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      await _emailService.SendPasswordResetEmailAsync(user.Email!, user.Name!, user.UserName!, token);
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
			ValidIssuer = _lgdxRobot2SecretConfiguration.LgdxUserJwtIssuer,
			ValidAudience = _lgdxRobot2SecretConfiguration.LgdxUserJwtIssuer,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_lgdxRobot2SecretConfiguration.LgdxUserJwtSecret)),
			ClockSkew = TimeSpan.Zero
		};
    ClaimsPrincipal principal = tokenHandler.ValidateToken(refreshTokenRequestBusinessModel.RefreshToken, validationParameters, out SecurityToken validatedToken) 
      ?? throw new LgdxValidation400Expection(nameof(refreshTokenRequestBusinessModel.RefreshToken), "Invalid refresh token.");

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
    var accessExpires = notBefore.AddMinutes(_lgdxRobot2SecretConfiguration.LgdxUserAccessTokenExpiresMins);
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
      ExpiresMins = _lgdxRobot2SecretConfiguration.LgdxUserAccessTokenExpiresMins
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
    return true;
  }
}