using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LGDXRobot2Cloud.API.Areas.Identity.Controllers;
[ApiController]
[Area("Identity")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController(
    IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
    UserManager<LgdxUser> userManager,
    ILgdxRoleRepository lgdxRoleRepository
  ) : ControllerBase
{
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobot2SecretConfiguration));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
  private readonly ILgdxRoleRepository _lgdxRoleRepository = lgdxRoleRepository ?? throw new ArgumentNullException(nameof(lgdxRoleRepository));

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

  [HttpPost("login")]
  [AllowAnonymous]
  public async Task<ActionResult<LoginResponse>>Login(LoginRequest loginDto)
  {
    var user = await _userManager.FindByNameAsync(loginDto.Username);
    if (user == null)
    {
      return Unauthorized("User not found.");
    }
    if (await _userManager.IsLockedOutAsync(user))
    {
      return Unauthorized("User is locked out.");
    }
    // Check password and generate token
    var token = await CheckPasswordAndGenerateTokenAsync(user, loginDto.Password);
    if (token != null)
    {
      // Password is correct
      return Ok(new LoginResponse {
        AccessToken = new JwtSecurityTokenHandler().WriteToken(token), 
        ExpiresMins = _lgdxRobot2SecretConfiguration.LgdxUserAccessTokenExpiresMins
      });
    }
    else
    {
      // Password is incorrect
      var incrementLockoutResult = await _userManager.AccessFailedAsync(user);
      if (!incrementLockoutResult.Succeeded)
      {
        // Return the same failure we do when resetting the lockout fails after a correct password.
        return Unauthorized();
      }
      if (await _userManager.IsLockedOutAsync(user))
      {
        return Unauthorized("User is locked out.");
      }
    }    
    return Unauthorized("Login failed.");
  }

  [HttpPost("updatePassword")]
  public async Task<ActionResult<LoginResponse>> UpdatePassword(UpdatePasswordRequest updatePasswordRequest)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await userManager.FindByIdAsync(userId!);
    if (user == null)
    {
      return NotFound();
    }

    var changePasswordResult = await _userManager.ChangePasswordAsync(user, updatePasswordRequest.OldPassword, updatePasswordRequest.NewPassword);
    if (!changePasswordResult.Succeeded)
    {
      return BadRequest();
    }
    
    return NoContent();
  }
}