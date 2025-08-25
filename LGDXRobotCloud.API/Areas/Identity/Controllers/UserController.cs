using System.Security.Claims;
using LGDXRobotCloud.API.Services.Identity;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Identity;
using LGDXRobotCloud.Data.Models.DTOs.V1.Commands;
using LGDXRobotCloud.Data.Models.DTOs.V1.Requests;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Utilities.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobotCloud.API.Areas.Identity.Controllers;

[ApiController]
[Area("Identity")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificateScheme)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController(
    IAuthService authService,
    ICurrentUserService currentUserService
  ) : ControllerBase
{
  private readonly IAuthService _authService = authService ?? throw new ArgumentNullException(nameof(authService));
  private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

  [HttpGet("")]
  [ProducesResponseType(typeof(LgdxUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<LgdxUserDto>> GetUser()
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await _currentUserService.GetUserAsync(userId!);
    return Ok(user.ToDto());
  }

  [HttpPut("")]
  [ProducesResponseType(typeof(LgdxUserDto), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateUser(LgdxUserUpdateDto lgdxUserUpdateDto)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    await _currentUserService.UpdateUserAsync(userId!, lgdxUserUpdateDto.ToBusinessModel());
    return NoContent();
  }

  [HttpPost("Password")]
  [ProducesResponseType(typeof(LgdxUserDto), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdatePassword(UpdatePasswordRequestDto updatePasswordRequestDto)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    await _authService.UpdatePasswordAsync(userId!, updatePasswordRequestDto.ToBusinessModel());
    return NoContent();
  }

  [HttpPost("2FA/Initiate")]
  [ProducesResponseType(typeof(InitiateTwoFactorResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<InitiateTwoFactorResponseDto>> InitiateTwoFactor()
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var respond = await _currentUserService.InitiateTwoFactorAsync(userId!);
    return Ok(new InitiateTwoFactorResponseDto {
      SharedKey = respond
    });
  }

  [HttpPost("2FA/Enable")]
  [ProducesResponseType(typeof(EnableTwoFactorRespondDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<EnableTwoFactorRespondDto>> EnableTwoFactor(EnableTwoFactorRequestDto enableTwoFactorRequestDto)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var respond = await _currentUserService.EnableTwoFactorAsync(userId!, enableTwoFactorRequestDto.TwoFactorCode);
    return Ok(new EnableTwoFactorRespondDto {
      RecoveryCodes = respond
    });
  }

  [HttpPost("2FA/Reset")]
  [ProducesResponseType(typeof(ResetRecoveryCodesRespondDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ResetRecoveryCodesRespondDto>> ResetRecoveryCodes()
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var respond = await _currentUserService.ResetRecoveryCodesAsync(userId!);
    return Ok(new ResetRecoveryCodesRespondDto {
      RecoveryCodes = respond
    });
  }

  [HttpPost("2FA/Disable")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DisableTwoFactor()
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    await _currentUserService.DisableTwoFactorAsync(userId!);
    return Ok();
  }
}