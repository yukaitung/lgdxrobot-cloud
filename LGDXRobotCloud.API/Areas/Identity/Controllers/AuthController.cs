using LGDXRobotCloud.API.Services.Identity;
using LGDXRobotCloud.Data.Models.Business.Identity;
using LGDXRobotCloud.Data.Models.DTOs.V1.Requests;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobotCloud.API.Areas.Identity.Controllers;

[ApiController]
[Area("Identity")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificationScheme)]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
  private readonly IAuthService _authService = authService ?? throw new ArgumentNullException(nameof(authService));

  [HttpPost("Login")]
  [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<LoginResponseDto>>Login(LoginRequestDto loginRequestDto)
  {
    var result = await _authService.LoginAsync(loginRequestDto.ToBusinessModel());
    return Ok(result.ToDto());
  }

  [HttpPost("ForgotPassword")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult> ForgotPassword(ForgotPasswordRequestDto forgotPasswordRequestDto)
  {
    await _authService.ForgotPasswordAsync(forgotPasswordRequestDto.ToBusinessModel());
    return Ok();
  }

  [HttpPost("ResetPassword")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> ResetPassword(ResetPasswordRequestDto resetPasswordRequestDto)
  {
    await _authService.ResetPasswordAsync(resetPasswordRequestDto.ToBusinessModel());
    return Ok();
  }

  [HttpPost("Refresh")]
  [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> Refresh(RefreshTokenRequestDto refreshTokenRequestDto)
  {
    var result = await _authService.RefreshTokenAsync(refreshTokenRequestDto.ToBusinessModel());
    return Ok(result.ToDto());
  }
}