using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Identify;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LGDXRobot2Cloud.API.Areas.Identify.Controllers;

[ApiController]
[Area("Identify")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController(
    IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
    SignInManager<LgdxUser> signInManager,
    UserManager<LgdxUser> userManager
  ) : ControllerBase
{
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobot2SecretConfiguration));
  private readonly SignInManager<LgdxUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
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

  [HttpPost("login")]
  [AllowAnonymous]
  public async Task<ActionResult<LoginResponse>>Login(LoginRequest loginDto)
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
        new (ClaimTypes.NameIdentifier, user.Id.ToString()),
        new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      };                
      foreach (var userRole in userRoles)
      {
        Claims.Add(new Claim(ClaimTypes.Role, userRole));
      }
      var accessToken = GenerateJwtToken(Claims, _lgdxRobot2SecretConfiguration.LgdxUserAccessTokenExpiresMins);
      return Ok(new LoginResponse {
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