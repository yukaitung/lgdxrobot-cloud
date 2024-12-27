using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    ILgdxRoleRepository lgdxRoleRepository,
    IMapper mapper,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    RoleManager<LgdxRole> roleManager
  ) : ControllerBase
{
  private readonly ILgdxRoleRepository _lgdxRoleRepository = lgdxRoleRepository ?? throw new ArgumentNullException(nameof(lgdxRoleRepository));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));
  private readonly RoleManager<LgdxRole> _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
  
  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<LgdxRoleListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<LgdxRoleListDto>>> GetRoles(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (roles, PaginationHelper) = await _lgdxRoleRepository.GetRolesAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<LgdxRoleListDto>>(roles));
  }

  [HttpGet("{id}", Name = "GetRole")]
  [ProducesResponseType(typeof(LgdxRoleDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<LgdxRoleDto>> GetRole(Guid id)
  {
    var role = await _roleManager.FindByIdAsync(id.ToString());
    if (role == null)
    {
      return NotFound();
    }
    var scopes = await _lgdxRoleRepository.GetRoleScopesAsync(role.Id);
    var lgdxRoleDto = _mapper.Map<LgdxRoleDto>(role);
    lgdxRoleDto.Scopes = scopes.Select(s => s.ClaimValue!);
    return Ok(lgdxRoleDto);
  }

  [HttpPost("")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateRole(LgdxRoleCreateDto lgdxRoleCreateDto)
  {
    var role = new LgdxRole
    {
      Name = lgdxRoleCreateDto.Name,
      Description = lgdxRoleCreateDto.Description,
      NormalizedName = lgdxRoleCreateDto.Name.ToUpper(),
    };
    var result = await _roleManager.CreateAsync(role);
    if (!result.Succeeded)
    {
      ModelState.AddModelError(nameof(LgdxRoleCreateDto), "Cannot create role.");
      return ValidationProblem();
    }
    if (lgdxRoleCreateDto.Scopes.Any())
    {
      var addScopeResult = await _lgdxRoleRepository.AddRoleScopesAsync(role, lgdxRoleCreateDto.Scopes);
      if (!addScopeResult)
      {
        ModelState.AddModelError(nameof(LgdxRoleCreateDto.Scopes), "Cannot create role with scopes.");
        return ValidationProblem();
      }
    }
    var roleEntity = await _roleManager.FindByNameAsync(lgdxRoleCreateDto.Name);
    var scopesEntity = await _lgdxRoleRepository.GetRoleScopesAsync(roleEntity!.Id);
    var lgdxRoleDto = _mapper.Map<LgdxRoleDto>(roleEntity);
    lgdxRoleDto.Scopes = scopesEntity.Select(r => r.ClaimValue!);
    return CreatedAtAction(nameof(GetRole), new { id = lgdxRoleDto.Id }, lgdxRoleDto);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateRole(Guid id, LgdxRoleUpdateDto lgdxRoleUpdateDto)
  {
    var roleEntity = await _roleManager.FindByIdAsync(id.ToString());
    if (roleEntity == null)
    {
      return NotFound();
    }
    _mapper.Map(lgdxRoleUpdateDto, roleEntity);
    var result = await _roleManager.UpdateAsync(roleEntity);
    if (!result.Succeeded)
    {
      ModelState.AddModelError(nameof(LgdxRoleCreateDto), "Cannot update role.");
      return ValidationProblem();
    }
    var scopesEntity = await _lgdxRoleRepository.GetRoleScopesAsync(roleEntity!.Id);
    var scopesList = scopesEntity.Select(s => s.ClaimValue).ToList();
    var scopeToAdd = lgdxRoleUpdateDto.Scopes.Except(scopesList);
    if (scopeToAdd.Any())
    {
      bool addResult = await _lgdxRoleRepository.AddRoleScopesAsync(roleEntity, scopeToAdd!);
      if (!addResult)
      {
        ModelState.AddModelError(nameof(LgdxRoleCreateDto.Scopes), "Cannot update role with scopes.");
        return ValidationProblem();
      }
    }
    var scopeToRemove = scopesList.Except(lgdxRoleUpdateDto.Scopes);
    if (scopeToRemove.Any())
    {
      bool removeResult = await _lgdxRoleRepository.RemoveRoleScopesAsync(roleEntity, scopeToRemove!);
      if (!removeResult)
      {
        ModelState.AddModelError(nameof(LgdxRoleCreateDto.Scopes), "Cannot update role with scopes.");
        return ValidationProblem();
      }
    }
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteRole(Guid id)
  {
    var roleEntity = await _roleManager.FindByIdAsync(id.ToString());
    if (roleEntity == null)
    {
      return NotFound();
    }
    var result = await _roleManager.DeleteAsync(roleEntity);
    if (!result.Succeeded)
    {
      ModelState.AddModelError(nameof(LgdxRoleCreateDto), "Cannot delete role.");
      return ValidationProblem();
    }
    return NoContent();
  }
}