using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
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
public sealed class AuthController(
    ILgdxRoleRepository lgdxRoleRepository,
    IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
    UserManager<LgdxUser> userManager
  ) : ControllerBase
{
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

  [AllowAnonymous]
  [HttpPost("Login")]
  [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<LoginResponseDto>>Login(LoginRequestDto loginRequestDto)
  {
    var user = await _userManager.FindByNameAsync(loginRequestDto.Username);
    if (user == null)
    {
      ModelState.AddModelError(nameof(LoginRequestDto.Username), "The user does not exist.");
      return ValidationProblem();
    }
    if (await _userManager.IsLockedOutAsync(user))
    {
      ModelState.AddModelError(nameof(LoginRequestDto.Username), "The user is locked out.");
      return ValidationProblem();
    }
    // Check password and generate token
    var token = await CheckPasswordAndGenerateTokenAsync(user, loginRequestDto.Password);
    if (token != null)
    {
      // Password is correct
      return Ok(new LoginResponseDto 
      {
        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
        RefreshToken = new JwtSecurityTokenHandler().WriteToken(token),
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
        ModelState.AddModelError(nameof(LoginRequestDto.Username), "Login failed.");
        return ValidationProblem();
      }
      if (await _userManager.IsLockedOutAsync(user))
      {
        ModelState.AddModelError(nameof(LoginRequestDto.Username), "The user is locked out.");
        return ValidationProblem();
      }
    }    
    ModelState.AddModelError(nameof(LoginRequestDto.Username), "Login failed.");
      return ValidationProblem();
  }

  [AllowAnonymous]
  [HttpPost("ForgotPassword")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult> ForgotPassword(ForgotPasswordRequestDto forgotPasswordRequestDto)
  {
    var user = await _userManager.FindByEmailAsync(forgotPasswordRequestDto.Email);
    if (user == null)
    {
      // For security reasons, we do not return a 404 status code.
      return Ok();
    }
    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    Console.WriteLine($"Your password reset link is ?Token={Convert.ToBase64String(Encoding.UTF8.GetBytes(token))}&Email={user.Email}");
    // TODO: Send email
    return Ok();
  }

  [AllowAnonymous]
  [HttpPost("ResetPassword")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> ResetPassword(ResetPasswordRequestDto resetPasswordRequestDto)
  {
    var user = await _userManager.FindByEmailAsync(resetPasswordRequestDto.Email);
    if (user == null)
    {
      // For security reasons, we do not return a 404 status code.
      ModelState.AddModelError(nameof(ResetPasswordRequestDto.Token), "");
      return ValidationProblem();
    }
    var result = await _userManager.ResetPasswordAsync(user, resetPasswordRequestDto.Token, resetPasswordRequestDto.NewPassword);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
    return Ok();
  }
}