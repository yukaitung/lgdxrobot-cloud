using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class MapsController(
  IMapper mapper,
  IMapRepository mapRepository
) : ControllerBase
{
  private readonly IMapRepository _mapRepository = mapRepository ?? throw new ArgumentNullException(nameof(mapRepository));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<MapListDto>>> GetMaps(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > 100) ? 100 : pageSize;
    var (maps, PaginationHelper) = await _mapRepository.GetMapsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<MapListDto>>(maps));
  }

  [HttpGet("{id}", Name = "GetMap")]
  public async Task<ActionResult<MapDto>> GetMap(int id)
  {
    var map = await _mapRepository.GetMapAsync(id);
    if (map == null)
      return NotFound();
    return Ok(_mapper.Map<MapDto>(map));
  }

  [HttpGet("default")]
  public async Task<ActionResult<MapDto>> GetDefaultMap()
  {
    var map = await _mapRepository.GetDefaultMapAsync();
    if (map == null)
      return NotFound();
    return Ok(_mapper.Map<MapDto>(map));
  }

  [HttpPost("")]
  public async Task<ActionResult> CreateMap(MapCreateDto mapDto)
  {
    var mapEntity = _mapper.Map<Map>(mapDto);
    await _mapRepository.AddMapAsync(mapEntity);
    await _mapRepository.SaveChangesAsync();
    var returnMap = _mapper.Map<MapDto>(mapEntity);
    return CreatedAtAction(nameof(GetMap), new { id = returnMap.Id }, returnMap);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateMap(int id, MapUpdateDto mapDto)
  {
    var mapEntity = await _mapRepository.GetMapAsync(id);
    if (mapEntity == null)
      return NotFound();
    _mapper.Map(mapDto, mapEntity);
    mapEntity.UpdatedAt = DateTime.UtcNow;
    await _mapRepository.SaveChangesAsync();
    var returnMap = _mapper.Map<MapDto>(mapEntity);
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteMap(int id)
  {
    var map = await _mapRepository.GetMapAsync(id);
    if (map == null)
      return NotFound();
    _mapRepository.DeleteMap(map);
    await _mapRepository.SaveChangesAsync();
    return NoContent();
  }
}