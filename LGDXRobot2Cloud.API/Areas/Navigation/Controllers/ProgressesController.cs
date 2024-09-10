using AutoMapper;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]

public class ProgressesController(
  IMapper mapper,
  IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
  IProgressRepository progressRepository) : ControllerBase
{
  private readonly IMapper _mapper = mapper;
  private readonly IProgressRepository _progressRepository = progressRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<ProgressDto>>> GetProgresses(string? name, int pageNumber = 1, int pageSize = 10, bool hideReserved = false, bool hideSystem = false)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (progresses, PaginationHelper) = await _progressRepository.GetProgressesAsync(name, pageNumber, pageSize, hideReserved, hideSystem);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<ProgressDto>>(progresses));
  }

  [HttpGet("{id}", Name = "GetProgress")]
  public async Task<ActionResult<Progress>> GetProgress(int id)
  {
    var progress = await _progressRepository.GetProgressAsync(id);
    if (progress == null)
      return NotFound();
    return Ok(_mapper.Map<ProgressDto>(progress));
  }

  [HttpPost("")]
  public async Task<ActionResult> CreateProgress(ProgressCreateDto progressDto)
  {
    var progressEntity = _mapper.Map<Progress>(progressDto);
    await _progressRepository.AddProgressAsync(progressEntity);
    await _progressRepository.SaveChangesAsync();
    var returnProgress = _mapper.Map<ProgressDto>(progressEntity);
    return CreatedAtRoute(nameof(GetProgress), new { id = returnProgress.Id }, returnProgress);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateProgress(int id, ProgressUpdateDto progressDto)
  {
    var progressEntity = await _progressRepository.GetProgressAsync(id);
    if (progressEntity == null)
      return NotFound();
    if (progressEntity.System)
      return BadRequest("Cannot update system defined progress.");
    _mapper.Map(progressDto, progressEntity);
    progressEntity.UpdatedAt = DateTime.UtcNow;
    await _progressRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteProgress(int id)
  {
    var progress = await _progressRepository.GetProgressAsync(id);
    if (progress == null)
      return NotFound();
    if (progress.System)
      return BadRequest("Cannot delete system defined progress.");
    _progressRepository.DeleteProgress(progress);
    await _progressRepository.SaveChangesAsync();
    return NoContent();
  }
}