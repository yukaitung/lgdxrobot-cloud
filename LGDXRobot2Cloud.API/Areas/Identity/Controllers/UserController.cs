using System.Security.Claims;
using AutoMapper;
using LGDXRobot2Cloud.API.Services.Common;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.Data.Models.Emails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Areas.Identity.Controllers;

[ApiController]
[Area("Identity")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class UserController(
    IEmailService emailService,
    IMapper mapper,
    UserManager<LgdxUser> userManager
  ) : ControllerBase
{
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  [HttpGet("")]
  [ProducesResponseType(typeof(LgdxUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<LgdxUserDto>> GetUser()
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await userManager.FindByIdAsync(userId!);
    if (user == null)
    {
      return NotFound();
    }
    var lgdxUserDto = _mapper.Map<LgdxUserDto>(user);
    lgdxUserDto.Roles = await _userManager.GetRolesAsync(user);
    return Ok(lgdxUserDto);
  }

  [HttpPut("")]
  [ProducesResponseType(typeof(LgdxUserDto), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateUser(LgdxUserUpdateDto lgdxUserUpdateDto)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var userEntity = await _userManager.FindByIdAsync(userId!.ToString());
    if (userEntity == null)
    {
      return NotFound();
    }
    _mapper.Map(lgdxUserUpdateDto, userEntity);
    var result = await _userManager.UpdateAsync(userEntity);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
    return NoContent();
  }

  [HttpPost("Password")]
  [ProducesResponseType(typeof(LgdxUserDto), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdatePassword(UpdatePasswordRequestDto updatePasswordRequestDto)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await userManager.FindByIdAsync(userId!);
    if (user == null)
    {
      return NotFound();
    }
    var result = await _userManager.ChangePasswordAsync(user, updatePasswordRequestDto.CurrentPassword, updatePasswordRequestDto.NewPassword);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(error.Code, error.Description);
      }
      return ValidationProblem();
    }
    await _emailService.SendPasswordUpdateEmailAsync(
      user.Email!,
      user.Name!,
      new PasswordUpdateViewModel {
        UserName = user.UserName!,
        Time = DateTime.Now.ToString("dd MMMM yyyy, hh:mm:ss tt")
      });
    return NoContent();
  }
}