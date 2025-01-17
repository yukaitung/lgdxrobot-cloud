using System.Text.Json;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Services.Navigation;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.Data.Models.Business.Navigation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class RealmsController(
  IRealmService realmService
) : ControllerBase
{
  private readonly IRealmService _realmService = realmService ?? throw new ArgumentNullException(nameof(realmService));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<RealmListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RealmListDto>>> GetRealms(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > 100) ? 100 : pageSize;
    var (realms, PaginationHelper) = await _realmService.GetRealmsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(realms.ToDto());
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<RealmSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RealmSearchDto>>> SearchRealms(string? name)
  {
    var realms = await _realmService.SearchRealmsAsync(name);
    return Ok(realms.ToDto());
  }

  [HttpGet("{id}", Name = "GetRealm")]
  [ProducesResponseType(typeof(RealmDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RealmDto>> GetRealm(int id)
  {
    var realm = await _realmService.GetRealmAsync(id);
    return Ok(realm.ToDto());
  }

  [HttpGet("Default")]
  [ProducesResponseType(typeof(RealmDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RealmDto>> GetDefaultRealm()
  {
    var realm = await _realmService.GetDefaultRealmAsync();
    return Ok(realm.ToDto());
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(RealmDto), StatusCodes.Status201Created)]
  public async Task<ActionResult> CreateRealm(RealmCreateDto realmCreateDto)
  {
    var realm = await _realmService.CreateRealmAsync(realmCreateDto.ToBusinessModel());
    return CreatedAtAction(nameof(GetRealm), new { id = realm.Id }, realm.ToDto());
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateRealm(int id, RealmUpdateDto realmUpdateDto)
  {
    if (!await _realmService.UpdateRealmAsync(id, realmUpdateDto.ToBusinessModel()))
    {
      return NotFound();
    }
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteRealm(int id)
  {
    if (!await _realmService.DeleteRealmAsync(id))
    {
      return NotFound();
    }
    return NoContent();
  }
}