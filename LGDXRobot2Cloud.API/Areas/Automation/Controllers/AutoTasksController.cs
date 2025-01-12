using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.Utilities.Enums;
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
public sealed class AutoTasksController(
    IAutoTaskRepository autoTaskRepository,
    IAutoTaskSchedulerService autoTaskSchedulerService,
    IFlowRepository flowRepository,
    IMapper mapper,
    IOnlineRobotsService onlineRobotsService,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    IProgressRepository progressRepository,
    IRealmRepository realmRepository,
    IRobotRepository robotRepository,
    IWaypointRepository waypointRepository
  ) : ControllerBase
{
  private readonly IAutoTaskRepository _autoTaskRepository = autoTaskRepository ?? throw new ArgumentNullException(nameof(autoTaskRepository));
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));
  private readonly IFlowRepository _flowRepository = flowRepository ?? throw new ArgumentNullException(nameof(flowRepository));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService ?? throw new ArgumentNullException(nameof(onlineRobotsService));
  private readonly IProgressRepository _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
  private readonly IRealmRepository _realmRepository = realmRepository ?? throw new ArgumentNullException(nameof(realmRepository));
  private readonly IRobotRepository _robotRepository = robotRepository ?? throw new ArgumentNullException(nameof(robotRepository));
  private readonly IWaypointRepository _waypointRepository = waypointRepository ?? throw new ArgumentNullException(nameof(waypointRepository)); 
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<AutoTaskListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<AutoTaskListDto>>> GetTasks(string? name, int? showProgressId, bool? showRunningTasks, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (tasks, PaginationHelper) = await _autoTaskRepository.GetAutoTasksAsync(name, showProgressId, showRunningTasks, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<AutoTaskListDto>>(tasks));
  }

  [HttpGet("{id}", Name = "GetTask")]
  [ProducesResponseType(typeof(AutoTaskDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<AutoTaskDto>> GetTask(int id)
  {
    var task = await _autoTaskRepository.GetAutoTaskAsync(id);
    if (task == null)
      return NotFound();
    return Ok(_mapper.Map<AutoTaskDto>(task));
  }

  private async Task<bool> ValidateAutoTask(HashSet<int> waypointIds, int flowId, int realmId, Guid? robotId)
  {
    bool isValid = true;
    // Validate the Waypoint IDs
    var waypointDict = await _waypointRepository.GetWaypointsDictFromListAsync(waypointIds);
    foreach(var waypointId in waypointIds)
    {
      if (!waypointDict.ContainsKey(waypointId))
      {
        ModelState.AddModelError(nameof(AutoTaskDetailDto.Waypoint), $"The Waypoint ID {waypointId} is invalid.");
        isValid = false;
      }
    }
    // Validate the Flow ID
    var flow = await _flowRepository.GetFlowAsync(flowId);
    if (flow == null)
    {
      ModelState.AddModelError(nameof(AutoTaskDto.Flow), $"The Flow ID {flowId} is invalid.");
      isValid = false;
    }
    // Validate the Realm ID
    var realm = await _realmRepository.GetRealmAsync(realmId);
    if (realm == null)
    {
      ModelState.AddModelError(nameof(AutoTaskDto.Realm), $"The Realm ID {realmId} is invalid.");
      isValid = false;
    }
    // Validate the Assigned Robot ID
    if (robotId != null) 
    {
      var robot = await _robotRepository.GetRobotSimpleAsync((Guid)robotId);
      if (robot == null)
      {
        ModelState.AddModelError(nameof(AutoTaskDto.AssignedRobot), $"Robot ID: {robotId} is invalid.");
        isValid = false;
      }
    }
    return isValid;
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(AutoTaskDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateTask(AutoTaskCreateDto autoTaskCreateDto)
  {
    HashSet<int> waypointIds = autoTaskCreateDto.AutoTaskDetails.Where(d => d.WaypointId != null).Select(d => d.WaypointId!.Value).ToHashSet();
    if (!await ValidateAutoTask(waypointIds, autoTaskCreateDto.FlowId, autoTaskCreateDto.RealmId, autoTaskCreateDto.AssignedRobotId))
      return ValidationProblem();
    var autoTaskEntity = _mapper.Map<AutoTask>(autoTaskCreateDto);
    // Add Progress to Entity
    autoTaskEntity.CurrentProgressId = autoTaskCreateDto.IsTemplate 
      ? (int)ProgressState.Template
      : (int)ProgressState.Waiting;
    await _autoTaskRepository.AddAutoTaskAsync(autoTaskEntity);
    await _autoTaskRepository.SaveChangesAsync();
    var autoTaskDto = _mapper.Map<AutoTaskDto>(autoTaskEntity);
    await _autoTaskSchedulerService.ResetIgnoreRobotAsync();
    return CreatedAtAction(nameof(GetTask), new {id = autoTaskDto.Id}, autoTaskDto);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateTask(int id, AutoTaskUpdateDto autoTaskUpdateDto)
  {
    var taskEntity = await _autoTaskRepository.GetAutoTaskAsync(id);
    if (taskEntity == null)
      return NotFound();
    if (taskEntity.CurrentProgressId != (int)ProgressState.Template)
    {
      ModelState.AddModelError(nameof(id), "Only AutoTask Templates are editable.");
      return ValidationProblem();
    }
    // Ensure the input task does not include Detail ID from other task
    HashSet<int> dbDetailIds = taskEntity.AutoTaskDetails.Select(d => d.Id).ToHashSet();
    foreach(var dtoDetailId in autoTaskUpdateDto.AutoTaskDetails.Where(d => d.Id != null).Select(d => d.Id))
    {
      if (dtoDetailId != null && !dbDetailIds.Contains((int)dtoDetailId))
      {
        ModelState.AddModelError(nameof(AutoTaskDetailDto.Id), $"The Task Detail ID {(int)dtoDetailId} is belongs to other Task.");
        return ValidationProblem();
      }
    }
    HashSet<int> waypointIds = autoTaskUpdateDto.AutoTaskDetails.Where(d => d.WaypointId != null).Select(d => d.WaypointId!.Value).ToHashSet();
    if (!await ValidateAutoTask(waypointIds, autoTaskUpdateDto.FlowId, autoTaskUpdateDto.RealmId, autoTaskUpdateDto.AssignedRobotId))
      return ValidationProblem();
    _mapper.Map(autoTaskUpdateDto, taskEntity);
    await _autoTaskRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteTask(int id)
  {
    var task = await _autoTaskRepository.GetAutoTaskAsync(id);
    if (task == null)
      return NotFound();
    if (task.CurrentProgressId != (int)ProgressState.Template)
    {
      ModelState.AddModelError(nameof(id), "Only task template can be deleted.");
      return ValidationProblem();
    }
    _autoTaskRepository.DeleteAutoTask(task);
    await _autoTaskRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpPost("{id}/Abort")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> AbortTask(int id)
  {
    var task = await _autoTaskRepository.GetAutoTaskAsync(id);
    if (task == null)
      return NotFound();
    if (task.CurrentProgressId == (int)ProgressState.Template || 
        task.CurrentProgressId == (int)ProgressState.Completed || 
        task.CurrentProgressId == (int)ProgressState.Aborted)
      {
        ModelState.AddModelError(nameof(id), "Cannot abort the task not in running status.");
        return ValidationProblem();
      }
    if (task.CurrentProgressId != (int)ProgressState.Waiting && 
        task.AssignedRobotId != null && 
        await _onlineRobotsService.SetAbortTaskAsync((Guid)task.AssignedRobotId!, true))
    {
      // If the robot is online, abort the task from the robot
      return NoContent();
    }
    await _autoTaskRepository.AutoTaskAbortManualAsync(task.Id);
    return NoContent();
  }

  [AllowAnonymous]
  [HttpPost("{id}/Next")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> AutoTaskNext(int id, AutoTaskNextDto autoTaskNextDto)
  {
    var result = await _autoTaskRepository.AutoTaskNextAsync(autoTaskNextDto.RobotId, id, autoTaskNextDto.NextToken);
    if (result == null)
    {
      ModelState.AddModelError(nameof(id), "The next token is invalid.");
      return ValidationProblem();
    }
    _onlineRobotsService.SetAutoTaskNext(autoTaskNextDto.RobotId, result);
    return NoContent();
  }
}