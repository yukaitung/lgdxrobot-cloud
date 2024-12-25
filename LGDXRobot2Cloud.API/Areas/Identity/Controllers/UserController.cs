using System.Security.Claims;
using AutoMapper;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Areas.Identity.Controllers;

[ApiController]
[Area("Identity")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController(
    IMapper mapper,
    UserManager<LgdxUser> userManager
  ) : ControllerBase
{
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly UserManager<LgdxUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

  [HttpGet("")]
  public async Task<ActionResult<LgdxUserDto>> GetUser()
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await userManager.FindByIdAsync(userId!);
    if (user == null)
    {
      return NotFound();
    }
    var returnDto = _mapper.Map<LgdxUserDto>(user);
    returnDto.Roles = await _userManager.GetRolesAsync(user);
    return Ok(returnDto);
  }

  [HttpPut("")]
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
      return BadRequest();
    }
    return NoContent();
  }

  [HttpPost("Password")]
  public async Task<ActionResult<LoginResponse>> UpdatePassword(UpdatePasswordRequest updatePasswordRequest)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await userManager.FindByIdAsync(userId!);
    if (user == null)
    {
      return NotFound();
    }

    var changePasswordResult = await _userManager.ChangePasswordAsync(user, updatePasswordRequest.CurrentPassword, updatePasswordRequest.NewPassword);
    if (!changePasswordResult.Succeeded)
    {
      return BadRequest();
    }
    
    return NoContent();
  }
}