using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.DTOs.V1.Commands;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Utilities.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;

namespace LGDXRobotCloud.API.Areas.Administration.Controllers;

[ApiController]
[Area("Administration")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificationScheme)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class UsersController(
    IOptionsSnapshot<LgdxRobotCloudConfiguration> lgdxRobotCloudConfiguration,
    IUserService userService
  ) : ControllerBase
{
  private readonly LgdxRobotCloudConfiguration _lgdxRobotCloudConfiguration = lgdxRobotCloudConfiguration.Value ?? throw new ArgumentNullException(nameof(_lgdxRobotCloudConfiguration));
  private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<LgdxUserListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<LgdxUserListDto>>> GetUsers(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobotCloudConfiguration.ApiMaxPageSize) ? _lgdxRobotCloudConfiguration.ApiMaxPageSize : pageSize;
    var (users, PaginationHelper) = await _userService.GetUsersAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(users.ToDto());
  }

  [HttpGet("{id}", Name = "GetUser")]
  [ProducesResponseType(typeof(LgdxUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<LgdxUserDto>> GetUser(Guid id)
  {
    var user = await _userService.GetUserAsync(id);
    return Ok(user.ToDto());
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(LgdxUserDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateUser(LgdxUserCreateAdminDto lgdxUserCreateAdminDto)
  {
    var user = await _userService.CreateUserAsync(lgdxUserCreateAdminDto.ToBusinessModel());
    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user.ToDto());
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateUser(Guid id, LgdxUserUpdateAdminDto lgdxUserUpdateAdminDto)
  {
    await _userService.UpdateUserAsync(id, lgdxUserUpdateAdminDto.ToBusinessModel());
    return NoContent();
  }

  [HttpPatch("{id}/Unlock")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UnlockUser(Guid id)
  {
    await _userService.UnlockUserAsync(id);
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteUser(Guid id)
  {
    var operatorId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    await _userService.DeleteUserAsync(id, operatorId!);
    return NoContent();
  }
}