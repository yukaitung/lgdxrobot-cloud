using System.Security.Claims;
using LGDXRobotCloud.API.Services.Identity;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.DTOs.V1.Commands;
using LGDXRobotCloud.Data.Models.DTOs.V1.Requests;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobotCloud.API.Areas.Identity.Controllers;

[ApiController]
[Area("Identity")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class UserController(
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
}