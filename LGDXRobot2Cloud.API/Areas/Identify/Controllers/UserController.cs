using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Requests;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;

namespace LGDXRobot2Cloud.API.Areas.Identify.Controllers;

[ApiController]
[Area("Identify")]
[Route("[area]/[controller]")]
public class UserController(UserManager<LgdxUser> userManager,
  SignInManager<LgdxUser> signInManager,
  IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
  IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
  TimeProvider timeProvider) : ControllerBase
{
  private readonly UserManager<LgdxUser> _userManager = userManager;
  private readonly SignInManager<LgdxUser> _signInManager = signInManager;
  private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions = bearerTokenOptions;
  private readonly TimeProvider _timeProvider = timeProvider;
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value;

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

  [HttpPost("login")]
  public async Task<ActionResult<LoginResponseDto>>Login(LoginDto loginDto)
  {
    var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, false, true);

    var user = await _userManager.FindByNameAsync(loginDto.Username);
    if (user == null)
    {
      return Unauthorized("User not found.");
    }
    if (await _userManager.IsLockedOutAsync(user))
    {
      return Unauthorized("User is locked out.");
    }
    if (await _userManager.CheckPasswordAsync(user, loginDto.Password))
    {
      var userRoles = await _userManager.GetRolesAsync(user);
      var Claims = new List<Claim>
      {
        new (ClaimTypes.NameIdentifier, user.UserName!),
        new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new (JwtRegisteredClaimNames.Sub, user.Email!)
      };                
      foreach (var userRole in userRoles)
      {
        Claims.Add(new Claim(ClaimTypes.Role, userRole));
      }
      var accessToken = GenerateJwtToken(Claims, _lgdxRobot2SecretConfiguration.LgdxUserAccessTokenExpiresMins);
      return Ok(new LoginResponseDto {
        AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken), 
        ExpiresMins = _lgdxRobot2SecretConfiguration.LgdxUserAccessTokenExpiresMins
      });
    }
    else
    {
      // If lockout is requested, increment access failed count which might lock out the user
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


}