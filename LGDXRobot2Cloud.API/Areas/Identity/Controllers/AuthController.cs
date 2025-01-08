using LGDXRobot2Cloud.API.Services.Identity;
using LGDXRobot2Cloud.Data.Models.Business.Identity;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Areas.Identity.Controllers;

[ApiController]
[Area("Identity")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
  private readonly IAuthService _authService = authService ?? throw new ArgumentNullException(nameof(authService));

  [AllowAnonymous]
  [HttpPost("Login")]
  [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<LoginResponseDto>>Login(LoginRequestDto loginRequestDto)
  {
    var result = await _authService.LoginAsync(loginRequestDto.ToBusinessModel());
    return Ok(result.ToDto());
  }

  [AllowAnonymous]
  [HttpPost("ForgotPassword")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult> ForgotPassword(ForgotPasswordRequestDto forgotPasswordRequestDto)
  {
    await _authService.ForgotPasswordAsync(forgotPasswordRequestDto.ToBusinessModel());
    return Ok();
  }

  [AllowAnonymous]
  [HttpPost("ResetPassword")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> ResetPassword(ResetPasswordRequestDto resetPasswordRequestDto)
  {
    await _authService.ResetPasswordAsync(resetPasswordRequestDto.ToBusinessModel());
    return Ok();
  }

  [AllowAnonymous]
  [HttpPost("Refresh")]
  [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> Refresh(RefreshTokenRequestDto refreshTokenRequestDto)
  {
    var result = await _authService.RefreshTokenAsync(refreshTokenRequestDto.ToBusinessModel());
    return Ok(result.ToDto());
  }
}