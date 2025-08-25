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
using System.Text.Json;

namespace LGDXRobotCloud.API.Areas.Administration.Controllers;

[ApiController]
[Area("Administration")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificateScheme)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class RolesController(
    IOptionsSnapshot<LgdxRobotCloudConfiguration> lgdxRobotCloudConfiguration,
    IRoleService roleService
  ) : ControllerBase
{
  private readonly LgdxRobotCloudConfiguration _lgdxRobotCloudConfiguration = lgdxRobotCloudConfiguration.Value ?? throw new ArgumentNullException(nameof(lgdxRobotCloudConfiguration));
  private readonly IRoleService _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
  
  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<LgdxRoleListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<LgdxRoleListDto>>> GetRoles(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobotCloudConfiguration.ApiMaxPageSize) ? _lgdxRobotCloudConfiguration.ApiMaxPageSize : pageSize;
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