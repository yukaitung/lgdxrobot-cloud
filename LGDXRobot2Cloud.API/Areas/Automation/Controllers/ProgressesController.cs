using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Automation.Controllers;

[ApiController]
[Area("Automation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class ProgressesController(
    IMapper mapper,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    IProgressRepository progressRepository
  ) : ControllerBase
{
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly IProgressRepository _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<ProgressDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ProgressDto>>> GetProgresses(string? name, int pageNumber = 1, int pageSize = 10, bool hideReserved = false, bool hideSystem = false)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (progresses, PaginationHelper) = await _progressRepository.GetProgressesAsync(name, pageNumber, pageSize, hideReserved, hideSystem);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<ProgressDto>>(progresses));
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<ProgressSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ProgressSearchDto>>> SearchProgresses(string name)
  {
    var progresses = await _progressRepository.SearchProgressesAsync(name);
    return Ok(_mapper.Map<IEnumerable<ProgressSearchDto>>(progresses));
  }

  [HttpGet("{id}", Name = "GetProgress")]
  [ProducesResponseType(typeof(ProgressDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ProgressDto>> GetProgress(int id)
  {
    var progress = await _progressRepository.GetProgressAsync(id);
    if (progress == null)
      return NotFound();
    return Ok(_mapper.Map<ProgressDto>(progress));
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(ProgressDto), StatusCodes.Status201Created)]
  public async Task<ActionResult> CreateProgress(ProgressCreateDto progressCreateDto)
  {
    var progressEntity = _mapper.Map<Progress>(progressCreateDto);
    await _progressRepository.AddProgressAsync(progressEntity);
    await _progressRepository.SaveChangesAsync();
    var progressDto = _mapper.Map<ProgressDto>(progressEntity);
    return CreatedAtRoute(nameof(GetProgress), new { id = progressDto.Id }, progressDto);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateProgress(int id, ProgressUpdateDto progressUpdateDto)
  {
    var progressEntity = await _progressRepository.GetProgressAsync(id);
    if (progressEntity == null)
      return NotFound();
    if (progressEntity.System)
    {
      ModelState.AddModelError(nameof(id), "Cannot update system progress.");
      return ValidationProblem();
    }
    _mapper.Map(progressUpdateDto, progressEntity);
    await _progressRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteProgress(int id)
  {
    var progress = await _progressRepository.GetProgressAsync(id);
    if (progress == null)
      return NotFound();
    if (progress.System)
    {
      ModelState.AddModelError(nameof(id), "Cannot delete system progress.");
      return ValidationProblem();
    }
    _progressRepository.DeleteProgress(progress);
    await _progressRepository.SaveChangesAsync();
    return NoContent();
  }
}