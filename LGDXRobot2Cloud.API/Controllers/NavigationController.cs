using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Shared.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class NavigationController : ControllerBase
  {
    private readonly IApiKeyLocationRepository _apiKeyLocationRepository;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IFlowRepository _flowRepository;
    private readonly IProgressRepository _progressRepository;
    private readonly IAutoTaskRepository _autoTaskRepository;
    private readonly ISystemComponentRepository _systemComponentRepository;
    private readonly ITriggerRepository _triggerRepository;
    private readonly IWaypointRepository _waypointRepository;
    private readonly IMapper _mapper;
    private readonly int maxPageSize = 100;

    public NavigationController(IApiKeyLocationRepository apiKeyLocationRepository,
      IApiKeyRepository apiKeyRepository,
      IFlowRepository flowRepository,
      IProgressRepository progressRepository,
      IAutoTaskRepository autoTaskRepository,
      ISystemComponentRepository systemComponentRepository,
      ITriggerRepository triggerRepository,
      IWaypointRepository waypointRepository,
      IMapper mapper)
    {
      _apiKeyLocationRepository = apiKeyLocationRepository ?? throw new ArgumentNullException(nameof(apiKeyLocationRepository));
      _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
      _flowRepository = flowRepository ?? throw new ArgumentNullException(nameof(flowRepository));
      _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
      _autoTaskRepository = autoTaskRepository ?? throw new ArgumentNullException(nameof(autoTaskRepository));
      _systemComponentRepository = systemComponentRepository ?? throw new ArgumentNullException(nameof(systemComponentRepository));
      _triggerRepository = triggerRepository ?? throw new ArgumentNullException(nameof(triggerRepository));
      _waypointRepository = waypointRepository ?? throw new ArgumentNullException(nameof(waypointRepository));
      _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /*
    ** Flow
    */
    [HttpGet("flows")]
    public async Task<ActionResult<IEnumerable<FlowListDto>>> GetFlows(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (flows, paginationMetadata) = await _flowRepository.GetFlowsAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
      return Ok(_mapper.Map<IEnumerable<FlowListDto>>(flows));
    }

    [HttpGet("flows/{id}", Name = "GetFlow")]
    public async Task<ActionResult<FlowDto>> GetFlow(int id)
    {
      var flow = await _flowRepository.GetFlowAsync(id);
      if (flow == null)
        return NotFound();
      return Ok(_mapper.Map<FlowDto>(flow));
    }

    [HttpPost("flows")]
    public async Task<ActionResult> CreateFlow(FlowCreateDto flowDto)
    {
      var flowEntity = _mapper.Map<Flow>(flowDto);
      // Validate Proceed Condition, add ProceedConditions Entity
      var proceedConditions = await _systemComponentRepository.GetSystemComponentsDictAsync();
      for (int i = 0; i < flowDto.FlowDetails.Count(); i++)
      {
        string temp = flowDto.FlowDetails.ElementAt(i).ProceedCondition;
        if (proceedConditions.ContainsKey(temp))
          flowEntity.FlowDetails.ElementAt(i).ProceedCondition = proceedConditions[temp];
        else
          return BadRequest($"The Proceed Condition {temp} is invalid.");
      }
      // Validate Progress & Trigger, add Entity for both
      HashSet<int> progressIds = new HashSet<int>();
      HashSet<int> triggerIds = new HashSet<int>();
      foreach (var detail in flowEntity.FlowDetails)
      {
        progressIds.Add(detail.ProgressId);
        if (detail.StartTriggerId != null)
          triggerIds.Add((int)detail.StartTriggerId);
        if (detail.EndTriggerId != null)
          triggerIds.Add((int)detail.EndTriggerId);
      }
      var progresses = await _progressRepository.GetProgressesDictFromListAsync(progressIds);
      var triggers = await _triggerRepository.GetTriggersDictFromListAsync(triggerIds);
      foreach (var detail in flowEntity.FlowDetails)
      {
        if (progresses.ContainsKey(detail.ProgressId))
          detail.Progress = progresses[detail.ProgressId];
        else
          return BadRequest($"The Progress Id: {detail.ProgressId} is invalid.");
        if (detail.StartTriggerId != null) // The detail has StartTriggerId
        {
          if (triggers.ContainsKey((int)detail.StartTriggerId))
            detail.StartTrigger = triggers[(int)detail.StartTriggerId];
          else
            return BadRequest($"The Start Trigger Id: {detail.StartTriggerId} is invalid.");
        }
        if (detail.EndTriggerId != null) // The detail has EndTriggerId
        {
          if (triggers.ContainsKey((int)detail.EndTriggerId))
            detail.EndTrigger = triggers[(int)detail.EndTriggerId];
          else
            return BadRequest($"The Start Trigger Id: {detail.EndTriggerId} does not exist.");
        }
      }
      await _flowRepository.AddFlowAsync(flowEntity);
      await _flowRepository.SaveChangesAsync();
      var returnFlow = _mapper.Map<FlowDto>(flowEntity);
      return CreatedAtAction(nameof(GetFlow), new { id = returnFlow.Id }, returnFlow);
    }

    [HttpPut("flows/{id}")]
    public async Task<ActionResult> UpdateFlow(int id, FlowUpdateDto flowDto)
    {
      var flowEntity = await _flowRepository.GetFlowAsync(id);
      if(flowEntity == null)
        return NotFound();
      // Ensure the input flow does not include Detail ID from other flow
      HashSet<int> detailIds = new HashSet<int>();
      foreach(var detail in flowEntity.FlowDetails)
        detailIds.Add(detail.Id);
      foreach(var detailDto in flowDto.FlowDetails)
      {
        if(detailDto.Id != null && !detailIds.Contains((int)detailDto.Id))
          return BadRequest($"The Flow Detail ID {(int)detailDto.Id} is belongs to other Flow.");
      }
      _mapper.Map(flowDto, flowEntity);
      // Validate Proceed Condition, add ProceedConditions Entity
      var proceedConditions = await _systemComponentRepository.GetSystemComponentsDictAsync();
      for (int i = 0; i < flowDto.FlowDetails.Count(); i++)
      {
        string temp = flowDto.FlowDetails.ElementAt(i).ProceedCondition;
        if (proceedConditions.ContainsKey(temp))
          flowEntity.FlowDetails.ElementAt(i).ProceedCondition = proceedConditions[temp];
        else
          return BadRequest($"The Proceed Condition: {temp} does not exist.");
      }
      // Validate Progress & Trigger, add Entity for both
      HashSet<int> progressIds = new HashSet<int>();
      HashSet<int> triggerIds = new HashSet<int>();
      foreach (var detail in flowEntity.FlowDetails)
      {
        progressIds.Add(detail.ProgressId);
        if (detail.StartTriggerId != null)
          triggerIds.Add((int)detail.StartTriggerId);
        if (detail.EndTriggerId != null)
          triggerIds.Add((int)detail.EndTriggerId);
      }
      var progresses = await _progressRepository.GetProgressesDictFromListAsync(progressIds);
      var triggers = await _triggerRepository.GetTriggersDictFromListAsync(triggerIds);
      foreach (var detail in flowEntity.FlowDetails)
      {
        if (progresses.ContainsKey(detail.ProgressId))
          detail.Progress = progresses[detail.ProgressId];
        else
          return BadRequest($"The Progress Id: {detail.ProgressId} does not exist.");
        if (detail.StartTriggerId != null) // The detail has StartTriggerId
        {
          if (triggers.ContainsKey((int)detail.StartTriggerId))
            detail.StartTrigger = triggers[(int)detail.StartTriggerId];
          else
            return BadRequest($"The Start Trigger Id: {detail.StartTriggerId} does not exist.");
        }
        if (detail.EndTriggerId != null) // The detail has EndTriggerId
        {
          if (triggers.ContainsKey((int)detail.EndTriggerId))
            detail.EndTrigger = triggers[(int)detail.EndTriggerId];
          else
            return BadRequest($"The Start Trigger Id: {detail.EndTriggerId} does not exist.");
        }
      }
      flowEntity.UpdatedAt = DateTime.UtcNow;
      await _flowRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("flows/{id}")]
    public async Task<ActionResult> DeleteFlow(int id)
    {
      var flow = await _flowRepository.GetFlowAsync(id);
      if (flow == null)
        return NotFound();
      _flowRepository.DeleteFlow(flow);
      await _flowRepository.SaveChangesAsync();
      return NoContent();
    }

    /*
    ** Progress
    */
    [HttpGet("progresses")]
    public async Task<ActionResult<IEnumerable<ProgressDto>>> GetProgresses(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (progresses, paginationMetadata) = await _progressRepository.GetProgressesAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
      return Ok(_mapper.Map<IEnumerable<ProgressDto>>(progresses));
    }

    [HttpGet("progresses/{id}", Name = "GetProgress")]
    public async Task<ActionResult<Progress>> GetProgress(int id)
    {
      var progress = await _progressRepository.GetProgressAsync(id);
      if (progress == null)
        return NotFound();
      return Ok(_mapper.Map<ProgressDto>(progress));
    }

    [HttpPost("progresses")]
    public async Task<ActionResult> CreateProgress(ProgressCreateDto progressDto)
    {
      var progressEntity = _mapper.Map<Progress>(progressDto);
      await _progressRepository.AddProgressAsync(progressEntity);
      await _progressRepository.SaveChangesAsync();
      var returnProgress = _mapper.Map<ProgressDto>(progressEntity);
      return CreatedAtRoute(nameof(GetProgress), new { id = returnProgress.Id }, returnProgress);
    }

    [HttpPut("progresses/{id}")]
    public async Task<ActionResult> UpdateProgress(int id, ProgressCreateDto progressDto)
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

    [HttpDelete("progresses/{id}")]
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

    /*
    ** Task
    */
    [HttpGet("tasks")]
    public async Task<ActionResult<IEnumerable<AutoTaskListDto>>> GetTasks(string? name, bool showSaved, bool showWaiting = true, bool showProcessing = true, 
      bool showCompleted = true, bool showAborted = true, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (tasks, paginationMetadata) = await _autoTaskRepository.GetAutoTasksAsync(name, showSaved, showWaiting, showProcessing, showCompleted, showAborted, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
      return Ok(_mapper.Map<IEnumerable<AutoTaskListDto>>(tasks));
    }

    [HttpGet("tasks/{id}", Name = "GetTask")]
    public async Task<ActionResult<AutoTaskDto>> GetTask(int id)
    {
      var task = await _autoTaskRepository.GetAutoTaskAsync(id);
      if (task == null)
        return NotFound();
      return Ok(_mapper.Map<AutoTaskDto>(task));
    }

    [HttpPost("tasks")]
    public async Task<ActionResult> CreateTask(AutoTaskCreateDto taskDto)
    {
      var taskEntity = _mapper.Map<AutoTask>(taskDto);
      // Validating the Waypoint ID
      HashSet<int> waypointIds = new HashSet<int>();
      foreach(var waypoint in taskDto.Waypoints)
        waypointIds.Add(waypoint.WaypointId);
      var waypointDict = await _waypointRepository.GetWaypointsDictFromListAsync(waypointIds);
      for(int i = 0; i < taskEntity.Waypoints.Count; i++)
      {
        if (waypointDict.ContainsKey(taskDto.Waypoints.ElementAt(i).WaypointId))
          taskEntity.Waypoints.ElementAt(i).Waypoint = waypointDict[taskDto.Waypoints.ElementAt(i).WaypointId];
        else
          return BadRequest($"The Waypoint ID {taskDto.Waypoints.ElementAt(i).WaypointId} is invalid.");
      }
      // Add Flow to Entity
      var flow = await _flowRepository.GetFlowAsync(taskEntity.FlowId);
      if (flow == null)
        return BadRequest($"The Flow ID {taskEntity.FlowId} is invalid.");
      taskEntity.Flow = flow;
      // Add Progress to Entity
      var currentProgress = taskDto.Saved ? 
        await _progressRepository.GetProgressAsync((int)ProgressState.Saved) : await _progressRepository.GetProgressAsync((int)ProgressState.Waiting);
      if (currentProgress == null)
        return BadRequest();
      taskEntity.CurrentProgress = currentProgress;
      taskEntity.CurrentProgressId = currentProgress.Id;
      await _autoTaskRepository.AddAutoTaskAsync(taskEntity);
      await _autoTaskRepository.SaveChangesAsync();
      var returnTask = _mapper.Map<AutoTaskDto>(taskEntity);
      return CreatedAtAction(nameof(GetTask), new {id = returnTask.Id}, returnTask);
    }

    [HttpPut("tasks/{id}")]
    public async Task<ActionResult> UpdateTask(int id, AutoTaskUpdateDto taskDto)
    {
      var taskEntity = await _autoTaskRepository.GetAutoTaskAsync(id);
      if (taskEntity == null)
        return NotFound();
      if (taskEntity.CurrentProgressId != (int)ProgressState.Waiting && taskEntity.CurrentProgressId != (int)ProgressState.Saved)
        return BadRequest($"Cannot change the task not in Waiting / Saved status.");
      // Ensure the input task does not include Detail ID from other task
      HashSet<int> detailIds = new HashSet<int>();
      foreach(var waypoint in taskEntity.Waypoints)
        detailIds.Add(waypoint.Id);
      foreach(var detailDto in taskDto.Waypoints)
      {
        if(detailDto.Id != null && !detailIds.Contains((int)detailDto.Id))
          return BadRequest($"The Task Waypoint Detail ID {(int)detailDto.Id} is belongs to other Flow.");
      }
      // Validating the Waypoint ID, Add Waypoint to Entity
      HashSet<int> waypointIds = new HashSet<int>();
      foreach(var waypoint in taskDto.Waypoints)
        waypointIds.Add(waypoint.WaypointId);
      var waypointDict = await _waypointRepository.GetWaypointsDictFromListAsync(waypointIds);
      for(int i = 0; i < taskDto.Waypoints.Count(); i++)
      {
        if (waypointDict.ContainsKey(taskDto.Waypoints.ElementAt(i).WaypointId))
          taskEntity.Waypoints.ElementAt(i).Waypoint = waypointDict[taskDto.Waypoints.ElementAt(i).WaypointId];
        else
          return BadRequest($"The Waypoint ID {taskDto.Waypoints.ElementAt(i).WaypointId} is invalid.");
      }
      _mapper.Map(taskDto, taskEntity);
      // Add Flow to Entity
      var flow = await _flowRepository.GetFlowAsync(taskEntity.FlowId);
      if (flow == null)
        return BadRequest($"The Flow ID {taskEntity.FlowId} is invalid.");
      taskEntity.Flow = flow;
      // Add Progress to Entity
      var currentProgress = await _progressRepository.GetProgressAsync(taskEntity.CurrentProgressId);
      if (currentProgress == null)
        return BadRequest();
      taskEntity.CurrentProgress = currentProgress;
      taskEntity.UpdatedAt = DateTime.UtcNow;
      await _autoTaskRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("tasks/{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
      var task = await _autoTaskRepository.GetAutoTaskAsync(id);
      if (task == null)
        return NotFound();
      if (task.CurrentProgressId != (int)ProgressState.Waiting && task.CurrentProgressId != (int)ProgressState.Saved)
        return BadRequest("Cannot delete the task not in Waiting status.");
      _autoTaskRepository.DeleteAutoTask(task);
      await _autoTaskRepository.SaveChangesAsync();
      return NoContent();
    }

    /*
    ** Trigger
    */
    [HttpGet("triggers")]
    public async Task<ActionResult<IEnumerable<TriggerDto>>> GetTriggers(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (triggers, paginationMetadata) = await _triggerRepository.GetTriggersAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
      return Ok(_mapper.Map<IEnumerable<TriggerDto>>(triggers));
    }

    [HttpGet("triggers/{id}", Name = "GetTrigger")]
    public async Task<ActionResult<TriggerDto>> GetTrigger(int id)
    {
      var trigger = await _triggerRepository.GetTriggerAsync(id);
      if (trigger == null)
        return NotFound();
      return Ok(_mapper.Map<TriggerDto>(trigger));
    }

    [HttpPost("triggers")]
    public async Task<ActionResult> CreateTrigger(TriggerCreateDto triggerDto)
    {
      var triggerEntity = _mapper.Map<Trigger>(triggerDto);
      var apiKeyLocation = await _apiKeyLocationRepository.GetApiKeyLocationAsync(triggerDto.ApiKeyLocationStr);
      if (apiKeyLocation == null)
        return BadRequest("The API key location is invalid.");
      triggerEntity.ApiKeyLocationId = apiKeyLocation.Id;
      var apiKey = await _apiKeyRepository.GetApiKeyAsync(triggerDto.ApiKeyId);
      if (apiKey == null)
        return BadRequest($"The API Key Id {triggerDto.ApiKeyId} is invalid.");
      if (!apiKey.IsThirdParty)
        return BadRequest("Only accept third party API key.");
      await _triggerRepository.AddTriggerAsync(triggerEntity);
      await _triggerRepository.SaveChangesAsync();
      var returnTrigger = _mapper.Map<TriggerDto>(triggerEntity);
      return CreatedAtAction(nameof(GetTrigger), new { id = returnTrigger.Id }, returnTrigger);
    }

    [HttpPut("triggers/{id}")]
    public async Task<ActionResult> UpdateTrigger(int id, TriggerCreateDto triggerDto)
    {
      var triggerEntity = await _triggerRepository.GetTriggerAsync(id);
      if (triggerEntity == null)
        return NotFound();
      _mapper.Map(triggerDto, triggerEntity);
      var apiKeyLocation = await _apiKeyLocationRepository.GetApiKeyLocationAsync(triggerDto.ApiKeyLocationStr);
      if (apiKeyLocation == null)
        return BadRequest("The API key location is invalid.");
      triggerEntity.ApiKeyLocationId = apiKeyLocation.Id;
      var apiKey = await _apiKeyRepository.GetApiKeyAsync(triggerDto.ApiKeyId);
      if (apiKey == null)
        return BadRequest($"The API Key Id {triggerDto.ApiKeyId} is invalid.");
      if (!apiKey.IsThirdParty)
        return BadRequest("Only accept third party API key.");
      triggerEntity.UpdatedAt = DateTime.UtcNow;
      await _triggerRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("triggers/{id}")]
    public async Task<ActionResult> DeleteTrigger(int id)
    {
      var trigger = await _triggerRepository.GetTriggerAsync(id);
      if (trigger == null)
        return NotFound();
      _triggerRepository.DeleteTrigger(trigger);
      await _triggerRepository.SaveChangesAsync();
      return NoContent();
    }

    /*
    ** Waypoint
    */
    [HttpGet("waypoints")]
    public async Task<ActionResult<IEnumerable<WaypointDto>>> GetWaypoints(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (waypoints, paginationMetadata) = await _waypointRepository.GetWaypointsAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
      return Ok(_mapper.Map<IEnumerable<WaypointDto>>(waypoints));
    }

    [HttpGet("waypoints/{id}", Name = "GetWaypoint")]
    public async Task<ActionResult<WaypointDto>> GetWaypoint(int id)
    {
      var waypoint = await _waypointRepository.GetWaypointAsync(id);
      if (waypoint == null)
        return NotFound();
      return Ok(_mapper.Map<WaypointDto>(waypoint));
    }

    [HttpPost("waypoints")]
    public async Task<ActionResult> CreateWaypoint(WaypointCreateDto waypointDto)
    {
      var waypointEntity = _mapper.Map<Waypoint>(waypointDto);
      await _waypointRepository.AddWaypointAsync(waypointEntity);
      await _waypointRepository.SaveChangesAsync();
      var returnWaypoint = _mapper.Map<WaypointDto>(waypointEntity);
      return CreatedAtRoute(nameof(GetWaypoint), new { id = returnWaypoint.Id }, returnWaypoint);
    }

    [HttpPut("waypoints/{id}")]
    public async Task<ActionResult> UpdateWaypoint(int id, WaypointUpdateDto waypointDto)
    {
      var waypointEntity = await _waypointRepository.GetWaypointAsync(id);
      if (waypointEntity == null)
        return NotFound();
      _mapper.Map(waypointDto, waypointEntity);
      waypointEntity.UpdatedAt = DateTime.UtcNow;
      await _waypointRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("waypoints/{id}")]
    public async Task<ActionResult> DeleteWaypoint(int id)
    {
      var waypoint = await _waypointRepository.GetWaypointAsync(id);
      if (waypoint == null)
        return NotFound();
      _waypointRepository.DeleteWaypoint(waypoint);
      await _waypointRepository.SaveChangesAsync();
      return NoContent();
    }
  }
}