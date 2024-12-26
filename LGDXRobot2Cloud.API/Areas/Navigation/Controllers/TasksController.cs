using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class TasksController(
  IAutoTaskRepository autoTaskRepository,
  IAutoTaskSchedulerService autoTaskSchedulerService,
  IFlowRepository flowRepository,
  IMapper mapper,
  IOnlineRobotsService onlineRobotsService,
  IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
  IProgressRepository progressRepository,
  IRobotRepository robotRepository,
  IWaypointRepository waypointRepository) : ControllerBase
{
  private readonly IAutoTaskRepository _autoTaskRepository = autoTaskRepository;
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService;
  private readonly IFlowRepository _flowRepository = flowRepository;
  private readonly IMapper _mapper = mapper;
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService;
  private readonly IProgressRepository _progressRepository = progressRepository;
  private readonly IRobotRepository _robotRepository = robotRepository;
  private readonly IWaypointRepository _waypointRepository = waypointRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<AutoTaskListDto>>> GetTasks(string? name, int? showProgressId, bool? showRunningTasks, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (tasks, PaginationHelper) = await _autoTaskRepository.GetAutoTasksAsync(name, showProgressId, showRunningTasks, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<AutoTaskListDto>>(tasks));
  }

  [HttpGet("{id}", Name = "GetTask")]
  public async Task<ActionResult<AutoTaskDto>> GetTask(int id)
  {
    var task = await _autoTaskRepository.GetAutoTaskAsync(id);
    if (task == null)
      return NotFound();
    return Ok(_mapper.Map<AutoTaskDto>(task));
  }

  private async Task<(AutoTask?, string)> ValidateAutoTask(AutoTaskUpdateDto taskDto)
  {
    var taskEntity = _mapper.Map<AutoTask>(taskDto);
    HashSet<int> waypointIds = [];
    // Creeate a Set of Waypoint IDs
    foreach(var waypointId in taskDto.Details.Select(d => d.WaypointId))
    {
      if (waypointId != null)
        waypointIds.Add((int)waypointId);
    }
    // Validate the Waypoint IDs
    var waypointDict = await _waypointRepository.GetWaypointsDictFromListAsync(waypointIds);
    for(int i = 0; i < taskEntity.Details.Count; i++)
    {
      var WaypointId = taskDto.Details.ElementAt(i).WaypointId;
      if (WaypointId != null && !waypointDict.ContainsKey((int)WaypointId))
      {
        return (null, $"The Waypoint ID {WaypointId} is invalid.");
      }
    }
    // Validate the Flow ID
    var flow = await _flowRepository.GetFlowAsync(taskEntity.FlowId);
    if (flow == null)
      return (null, $"The Flow ID {taskEntity.FlowId} is invalid.");
    // Validate the Assigned Robot ID
    if (taskEntity.AssignedRobotId != null) 
    {
      var robot = await _robotRepository.GetRobotSimpleAsync((Guid)taskEntity.AssignedRobotId!);
      if (robot == null)
        return (null, $"Robot ID: {taskEntity.AssignedRobotId} is invalid.");
    }
    return (taskEntity, string.Empty);
  }

  [HttpPost("")]
  public async Task<ActionResult> CreateTask(AutoTaskCreateDto taskDto)
  {
    var (taskEntity, error) = await ValidateAutoTask(_mapper.Map<AutoTaskUpdateDto>(taskDto));
    if (taskEntity == null)
      return BadRequest(error);
    // Add Progress to Entity
    var currentProgress = taskDto.IsTemplate 
      ? await _progressRepository.GetProgressAsync((int)ProgressState.Template) 
      : await _progressRepository.GetProgressAsync((int)ProgressState.Waiting);
    if (currentProgress == null)
      return BadRequest("The current progress is invalid.");
    taskEntity.CurrentProgressId = currentProgress.Id;
    await _autoTaskRepository.AddAutoTaskAsync(taskEntity);
    await _autoTaskRepository.SaveChangesAsync();
    var returnTask = _mapper.Map<AutoTaskDto>(taskEntity);
    await _autoTaskSchedulerService.ResetIgnoreRobotAsync();
    return CreatedAtAction(nameof(GetTask), new {id = returnTask.Id}, returnTask);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateTask(int id, AutoTaskUpdateDto taskDto)
  {
    var taskEntity = await _autoTaskRepository.GetAutoTaskAsync(id);
    if (taskEntity == null)
      return NotFound();
    if (taskEntity.CurrentProgressId != (int)ProgressState.Template)
      return BadRequest($"Only AutoTask Templates are editable.");
    // Ensure the input task does not include Detail ID from other task
    HashSet<int> detailIds = [];
    foreach(var detail in taskEntity.Details)
      detailIds.Add(detail.Id);
    foreach(var detailId in taskDto.Details.Select(d => d.Id))
    {
      if (detailId != null && !detailIds.Contains((int)detailId))
        return BadRequest($"The Task Detail ID {(int)detailId} is belongs to other Task.");
    }
    var (newTaskEntity, error) = await ValidateAutoTask(_mapper.Map<AutoTaskUpdateDto>(taskDto));
    if (newTaskEntity == null)
      return BadRequest(error);
    _mapper.Map(taskDto, taskEntity);
    // Add Progress to Entity
    var currentProgress = await _progressRepository.GetProgressAsync(taskEntity.CurrentProgressId);
    if (currentProgress == null)
      return BadRequest("The current progress is invalid.");
    taskEntity.CurrentProgressId = currentProgress.Id;
    taskEntity.UpdatedAt = DateTime.UtcNow;
    await _autoTaskRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteTask(int id)
  {
    var task = await _autoTaskRepository.GetAutoTaskAsync(id);
    if (task == null)
      return NotFound();
    if (task.CurrentProgressId != (int)ProgressState.Template)
      return BadRequest("Only task template can be deleted.");
    _autoTaskRepository.DeleteAutoTask(task);
    await _autoTaskRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpPost("{id}/abort")]
  public async Task<ActionResult> AbortTask(int id)
  {
    var task = await _autoTaskRepository.GetAutoTaskAsync(id);
    if (task == null)
      return NotFound();
    if (task.CurrentProgressId == (int)ProgressState.Template || 
        task.CurrentProgressId == (int)ProgressState.Completed || 
        task.CurrentProgressId == (int)ProgressState.Aborted)
      return BadRequest("Cannot abort the task not in running status.");
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
  public async Task<IActionResult> AutoTaskNext(int id, AutoTaskNextDto autoTaskNextDto)
  {
    var result = await _autoTaskRepository.AutoTaskNextAsync(autoTaskNextDto.RobotId, id, autoTaskNextDto.NextToken);
    if (result == null)
      return BadRequest("The next token is invalid.");

    _onlineRobotsService.SetAutoTaskNext(autoTaskNextDto.RobotId, result);
    return NoContent();
  }
}