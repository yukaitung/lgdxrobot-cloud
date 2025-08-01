using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.API.Services.Automation;

public interface IFlowService
{
  Task<(IEnumerable<FlowListBusinessModel>, PaginationHelper)> GetFlowsAsync(string? name, int pageNumber, int pageSize);
  Task<FlowBusinessModel> GetFlowAsync(int flowId);
  Task<FlowBusinessModel> CreateFlowAsync(FlowCreateBusinessModel flowCreateBusinessModel);
  Task<bool> UpdateFlowAsync(int flowId, FlowUpdateBusinessModel flowUpdateBusinessModel);
  Task<bool> TestDeleteFlowAsync(int flowId);
  Task<bool> DeleteFlowAsync(int flowId);

  Task<IEnumerable<FlowSearchBusinessModel>> SearchFlowsAsync(string? name);
}

public class FlowService(
    IActivityLogService activityLogService,
    LgdxContext context
  ) : IFlowService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<FlowListBusinessModel>, PaginationHelper)> GetFlowsAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Flows as IQueryable<Flow>;
      if(!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(f => f.Name.ToLower().Contains(name.ToLower()));
      }
      var itemCount = await query.CountAsync();
      var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
      var flows = await query.AsNoTracking()
        .OrderBy(t => t.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .Select(t => new FlowListBusinessModel {
          Id = t.Id,
          Name = t.Name,
        })
        .ToListAsync();
      return (flows, PaginationHelper);
  }

  public async Task<FlowBusinessModel> GetFlowAsync(int flowId)
  {
    return await _context.Flows.Where(f => f.Id == flowId)
      .Include(f => f.FlowDetails
        .OrderBy(fd => fd.Order))
        .ThenInclude(fd => fd.Progress)
      .Include(f => f.FlowDetails)
        .ThenInclude(fd => fd.Trigger)
      .Select(f => new FlowBusinessModel {
        Id = f.Id,
        Name = f.Name,
        FlowDetails = f.FlowDetails.Select(fd => new FlowDetailBusinessModel {
          Id = fd.Id,
          Order = fd.Order,
          ProgressId = fd.Progress.Id,
          ProgressName = fd.Progress.Name,
          AutoTaskNextControllerId = fd.AutoTaskNextControllerId,
          TriggerId = fd.Trigger!.Id,
          TriggerName = fd.Trigger!.Name,
        }).OrderBy(fd => fd.Order).ToList(),
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  private async Task ValidateFlow(HashSet<int> progressIds, HashSet<int> triggerIds)
  {
    var progresses = await _context.Progresses.Where(p => progressIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
    foreach (var progressId in progressIds)
    {
      if (progresses.TryGetValue(progressId, out var progress))
      {
        if (progress.Reserved)
        {
          throw new LgdxValidation400Expection("Progress", $"The Progress ID: {progressId} is reserved.");
        }
      }
      else
      {
        throw new LgdxValidation400Expection("Progress", $"The Progress Id: {progressId} is invalid.");
      }
    }
    var triggers = await _context.Triggers.Where(t => triggerIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id);
    foreach (var triggerId in triggerIds)
    {
      if (!triggers.TryGetValue(triggerId, out var trigger))
      {
        throw new LgdxValidation400Expection("Trigger", $"The Trigger ID: {triggerId} is invalid.");
      }
    }
  }

  public async Task<FlowBusinessModel> CreateFlowAsync(FlowCreateBusinessModel flowCreateBusinessModel)
  {
    HashSet<int> progressIds = flowCreateBusinessModel.FlowDetails.Select(d => d.ProgressId).ToHashSet();
    HashSet<int> triggerIds = flowCreateBusinessModel.FlowDetails.Where(d => d.TriggerId != null).Select(d => d.TriggerId!.Value).ToHashSet();
    await ValidateFlow(progressIds, triggerIds);
    var flow = new Flow {
      Name = flowCreateBusinessModel.Name,
      FlowDetails = flowCreateBusinessModel.FlowDetails.Select(fd => new FlowDetail {
        Order = fd.Order,
        ProgressId = fd.ProgressId,
        AutoTaskNextControllerId = fd.AutoTaskNextControllerId,
        TriggerId = fd.TriggerId,
      }).ToList(),
    };
    await _context.Flows.AddAsync(flow);
    await _context.SaveChangesAsync();
    
    await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(Flow),
      EntityId = flow.Id.ToString(),
      Action = ActivityAction.Create,
    });

    return new FlowBusinessModel
    {
      Id = flow.Id,
      Name = flow.Name,
      FlowDetails = flow.FlowDetails.Select(fd => new FlowDetailBusinessModel
      {
        Id = fd.Id,
        Order = fd.Order,
        ProgressId = fd.ProgressId,
        ProgressName = fd.Progress.Name,
        AutoTaskNextControllerId = fd.AutoTaskNextControllerId,
        TriggerId = fd.TriggerId,
        TriggerName = fd.Trigger?.Name,
      }).ToList(),
    };
  }

  public async Task<bool> UpdateFlowAsync(int flowId, FlowUpdateBusinessModel flowUpdateBusinessModel)
  {
    var flow = await _context.Flows.Where(f => f.Id == flowId)
      .Include(f => f.FlowDetails
        .OrderBy(fd => fd.Order))
        .ThenInclude(fd => fd.Progress)
      .Include(f => f.FlowDetails)
        .ThenInclude(fd => fd.Trigger)
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
    
    HashSet<int?> dbDetailIds = flowUpdateBusinessModel.FlowDetails.Where(d => d.Id != null).Select(d => d.Id).ToHashSet();
    foreach(var dtoDetailId in flowUpdateBusinessModel.FlowDetails.Where(d => d.Id != null).Select(d => d.Id))
    {
      if(!dbDetailIds.Contains((int)dtoDetailId!))
      {
        throw new LgdxValidation400Expection("FlowDetails", $"The Flow Detail ID {(int)dtoDetailId} is belongs to other Flow.");
      }
    }
    HashSet<int> progressIds = flowUpdateBusinessModel.FlowDetails.Select(d => d.ProgressId).ToHashSet();
    HashSet<int> triggerIds = flowUpdateBusinessModel.FlowDetails.Where(d => d.TriggerId != null).Select(d => d.TriggerId!.Value).ToHashSet();
    await ValidateFlow(progressIds, triggerIds);

    flow.Name = flowUpdateBusinessModel.Name;
    flow.FlowDetails = flowUpdateBusinessModel.FlowDetails.Select(fd => new FlowDetail {
        Id = (int)fd.Id!,
        Order = fd.Order,
        ProgressId = fd.ProgressId,
        AutoTaskNextControllerId = fd.AutoTaskNextControllerId,
        TriggerId = fd.TriggerId,
      }).ToList();
    await _context.SaveChangesAsync();

    await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(Flow),
      EntityId = flowId.ToString(),
      Action = ActivityAction.Update,
    });

    return true;
  }

  public async Task<bool> TestDeleteFlowAsync(int flowId)
  {
    var dependencies = await _context.AutoTasks
      .Where(a => a.FlowId == flowId)
      .Where(a => a.CurrentProgressId != (int)ProgressState.Completed && a.CurrentProgressId != (int)ProgressState.Aborted)
      .CountAsync();
    if (dependencies > 0)
    {
      throw new LgdxValidation400Expection(nameof(flowId), $"This flow has been used by {dependencies} running/waiting/template tasks.");
    }
    return true;
  }

  public async Task<bool> DeleteFlowAsync(int flowId)
  {
    var result = await _context.Flows.Where(f => f.Id == flowId)
      .ExecuteDeleteAsync() == 1;

    if (result)
    {
      await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Flow),
        EntityId = flowId.ToString(),
        Action = ActivityAction.Delete,
      });
    }
    return result;
  }

  public async Task<IEnumerable<FlowSearchBusinessModel>> SearchFlowsAsync(string? name)
  {
    var n = name ?? string.Empty;
    return await _context.Flows.AsNoTracking()
      .Where(w => w.Name.ToLower().Contains(n.ToLower()))
      .Take(10)
      .Select(t => new FlowSearchBusinessModel {
        Id = t.Id,
        Name = t.Name,
      })
      .ToListAsync();
  }
}