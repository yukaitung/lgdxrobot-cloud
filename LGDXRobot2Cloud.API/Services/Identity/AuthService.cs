using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services.Common;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Business.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LGDXRobot2Cloud.API.Services.Identity;

public interface IAuthService
{
  Task<LoginResponseBusinessModel> LoginAsync(LoginRequestBusinessModel loginRequestBusinessModel);
  Task ForgotPasswordAsync(ForgotPasswordRequestBusinessModel forgotPasswordRequestBusinessModel);
  Task ResetPasswordAsync(ResetPasswordRequestBusinessModel resetPasswordRequestBusinessModel);
  Task<RefreshTokenResponseBusinessModel> RefreshTokenAsync(RefreshTokenRequestBusinessModel refreshTokenRequestBusinessModel);
}

public class AuthService(
    IEmailService emailService,
    ILgdxRoleRepository lgdxRoleRepository,
    IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
    UserManager<LgdxUser> userManager
  ) : IAuthService
{
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly ILgdxRoleRepository _lgdxRoleRepository = lgdxRoleRepository ?? throw new ArgumentNullException(nameof(lgdxRoleRepository));
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobot2SecretConfiguration));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  private JwtSecurityToken GenerateJwtToken(List<Claim> claims, int expiresMins)
  {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_lgdxRobot2SecretConfiguration.LgdxUserJwtSecret));
    var credentials = new SigningCredentials(securityKey, _lgdxRobot2SecretConfiguration.LgdxUserJwtAlgorithm);
    return new JwtSecurityToken(
      _lgdxRobot2SecretConfiguration.LgdxUserJwtIssuer,
      _lgdxRobot2SecretConfiguration.LgdxUserJwtIssuer,
      claims,
      DateTime.UtcNow,
      DateTime.UtcNow.AddMinutes(expiresMins),
      credentials);
  }

  private async Task<JwtSecurityToken?> CheckPasswordAndGenerateTokenAsync(LgdxUser user, string password)
  {
    if (await _userManager.CheckPasswordAsync(user, password))
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
        var roles = await _lgdxRoleRepository.GetRolesAsync(userRoles);
        List<string> roleIds = roles.Select(r => r.Id).ToList();
        var roleClaims = await _lgdxRoleRepository.GetRolesClaimsAsync(roleIds);
        foreach (var roleClaim in roleClaims)
        {
          Claims.Add(new Claim(roleClaim.ClaimType!, roleClaim.ClaimValue!));
        }
      }
      return GenerateJwtToken(Claims, _lgdxRobot2SecretConfiguration.LgdxUserAccessTokenExpiresMins);
    }
    else
    {
      return null;
    }
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
    var token = await CheckPasswordAndGenerateTokenAsync(user, loginRequestBusinessModel.Password);
    if (token != null)
    {
      // Password is correct
      return new LoginResponseBusinessModel 
      {
        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
        RefreshToken = new JwtSecurityTokenHandler().WriteToken(token),
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
        foreach (var error in incrementLockoutResult.Errors)
        {
          throw new LgdxValidation400Expection(error.Code, error.Description);
        }
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
      foreach (var error in result.Errors)
      {
        throw new LgdxValidation400Expection(error.Code, error.Description);
      }
    }
    await _emailService.SendPasswordUpdateEmailAsync(user.Email!, user.Name!, user.UserName!);
  }

  public async Task<RefreshTokenResponseBusinessModel> RefreshTokenAsync(RefreshTokenRequestBusinessModel refreshTokenRequestBusinessModel)
  {
    throw new NotImplementedException();
  }
}