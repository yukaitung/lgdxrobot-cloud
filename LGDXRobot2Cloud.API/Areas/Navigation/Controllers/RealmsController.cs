using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
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
  IMapper mapper,
  IRealmRepository realmRepository
) : ControllerBase
{
  private readonly IRealmRepository _realmRepository = realmRepository ?? throw new ArgumentNullException(nameof(realmRepository));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<RealmListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RealmListDto>>> GetRealms(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > 100) ? 100 : pageSize;
    var (realms, PaginationHelper) = await _realmRepository.GetRealmsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<RealmListDto>>(realms));
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<RealmSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RealmSearchDto>>> SearchRealms(string name)
  {
    var realms = await realmRepository.SearchRealmsAsync(name);
    return Ok(_mapper.Map<IEnumerable<RealmSearchDto>>(realms));
  }

  [HttpGet("{id}", Name = "GetRealm")]
  [ProducesResponseType(typeof(RealmDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RealmDto>> GetRealm(int id)
  {
    var realm = await _realmRepository.GetRealmAsync(id);
    if (realm == null)
      return NotFound();
    return Ok(_mapper.Map<RealmDto>(realm));
  }

  [HttpGet("Default")]
  [ProducesResponseType(typeof(RealmDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RealmDto>> GetDefaultRealm()
  {
    var realm = await _realmRepository.GetDefaultRealmAsync();
    if (realm == null)
      return NotFound();
    return Ok(_mapper.Map<RealmDto>(realm));
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(RealmDto), StatusCodes.Status201Created)]
  public async Task<ActionResult> CreateRealm(RealmCreateDto realmCreateDto)
  {
    var realmEntity = _mapper.Map<Realm>(realmCreateDto);
    await _realmRepository.AddRealmAsync(realmEntity);
    await _realmRepository.SaveChangesAsync();
    var realmDto = _mapper.Map<RealmDto>(realmEntity);
    return CreatedAtAction(nameof(GetRealm), new { id = realmDto.Id }, realmDto);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateRealm(int id, RealmUpdateDto realmUpdateDto)
  {
    var realmEntity = await _realmRepository.GetRealmAsync(id);
    if (realmEntity == null)
      return NotFound();
    _mapper.Map(realmUpdateDto, realmEntity);
    await _realmRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteRealm(int id)
  {
    var realm = await _realmRepository.GetRealmAsync(id);
    if (realm == null)
      return NotFound();
    _realmRepository.DeleteRealm(realm);
    await _realmRepository.SaveChangesAsync();
    return NoContent();
  }
}