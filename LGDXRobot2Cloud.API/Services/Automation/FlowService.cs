using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Business.Automation;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Services.Automation;

public interface IFlowService
{
  Task<(IEnumerable<FlowListBusinessModel>, PaginationHelper)> GetFlowsAsync(string? name, int pageNumber, int pageSize);
  Task<FlowBusinessModel> GetFlowAsync(int flowId);
  Task<FlowBusinessModel> CreateFlowAsync(FlowCreateBusinessModel flowCreateBusinessModel);
  Task<bool> UpdateFlowAsync(int flowId, FlowUpdateBusinessModel flowUpdateBusinessModel);
  Task<bool> DeleteFlowAsync(int flowId);

  Task<IEnumerable<FlowSearchBusinessModel>> SearchFlowsAsync(string? name);
}

public class FlowService(LgdxContext context) : IFlowService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<FlowListBusinessModel>, PaginationHelper)> GetFlowsAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Flows as IQueryable<Flow>;
      if(!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(f => f.Name.Contains(name));
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
        }).ToList(),
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

    return new FlowBusinessModel {
      Id = flow.Id,
      Name = flow.Name,
      FlowDetails = flow.FlowDetails.Select(fd => new FlowDetailBusinessModel {
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
        Order = fd.Order,
        ProgressId = fd.ProgressId,
        AutoTaskNextControllerId = fd.AutoTaskNextControllerId,
        TriggerId = fd.TriggerId,
      }).ToList();
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<bool> DeleteFlowAsync(int flowId)
  {
    return await _context.Flows.Where(f => f.Id == flowId)
      .ExecuteDeleteAsync() > 1;
  }

  public async Task<IEnumerable<FlowSearchBusinessModel>> SearchFlowsAsync(string? name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return await _context.Flows.AsNoTracking()
        .Take(10)
        .Select(t => new FlowSearchBusinessModel {
          Id = t.Id,
          Name = t.Name,
        })
        .ToListAsync();
    }
    else
    {
      return await _context.Flows.AsNoTracking()
        .Where(w => w.Name.Contains(name))
        .Take(10)
        .Select(t => new FlowSearchBusinessModel {
          Id = t.Id,
          Name = t.Name,
        })
        .ToListAsync();
    }
  }
}