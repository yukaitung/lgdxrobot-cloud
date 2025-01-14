using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Services.Administration;
using LGDXRobot2Cloud.Data.Models.Business.Administration;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Administration.Controllers;

[ApiController]
[Area("Administration")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class RolesController(
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    IRoleService roleService
  ) : ControllerBase
{
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));
  private readonly IRoleService _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
  
  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<LgdxRoleListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<LgdxRoleListDto>>> GetRoles(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (roles, PaginationHelper) = await _roleService.GetRolesAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(roles.ToDto());
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<LgdxRoleSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<LgdxRoleSearchDto>>> SearchRoles(string name)
  {
    var roles = await _roleService.SearchRoleAsync(name);
    return Ok(roles.ToDto());
  }

  [HttpGet("{id}", Name = "GetRole")]
  [ProducesResponseType(typeof(LgdxRoleDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<LgdxRoleDto>> GetRole(Guid id)
  {
    var role = await _roleService.GetRoleAsync(id);
    return Ok(role.ToDto());
  }

  [HttpPost("")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateRole(LgdxRoleCreateDto lgdxRoleCreateDto)
  {
    var role = await _roleService.CreateRoleAsync(lgdxRoleCreateDto.ToBusinessModel());
    return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role.ToDto());
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateRole(Guid id, LgdxRoleUpdateDto lgdxRoleUpdateDto)
  {
    await _roleService.UpdateRoleAsync(id, lgdxRoleUpdateDto.ToBusinessModel());
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteRole(Guid id)
  {
    await _roleService.DeleteRoleAsync(id);
    return NoContent();
  }
}