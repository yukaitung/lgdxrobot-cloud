using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.API.Services.Automation;

public interface ITriggerRetryService
{
  Task<(IEnumerable<TriggerRetryListBusinessModel>, PaginationHelper)> GetTriggerRetriesAsync(int pageNumber, int pageSize);
  Task<TriggerRetryBusinessModel> GetTriggerRetryAsync(int triggerRetryId);
  Task<bool> DeleteTriggerRetryAsync(int triggerRetryId);
  Task RetryTriggerRetryAsync(int triggerRetryId);
  Task RetryAllFailedTriggerAsync(int triggerId);
}

public class TriggerRetryService (
  IActivityLogService activityService,
  ITriggerService triggerService,
  LgdxContext context
) : ITriggerRetryService
{
  private readonly IActivityLogService _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
  private readonly ITriggerService _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<TriggerRetryListBusinessModel>, PaginationHelper)> GetTriggerRetriesAsync(int pageNumber, int pageSize)
  {
    var triggerRetries = await _context.TriggerRetries.AsNoTracking()
      .Include(tr => tr.Trigger)
      .Include(tr => tr.AutoTask)
      .AsSplitQuery()
      .Select(tr => new TriggerRetryListBusinessModel {
        Id = tr.Id,
        TriggerId = tr.TriggerId,
        TriggerName = tr.Trigger.Name,
        AutoTaskId = tr.AutoTaskId,
        AutoTaskName = tr.AutoTask.Name,
        CreatedAt = tr.CreatedAt
      })
      .OrderBy(t => t.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .ToListAsync();
      var itemCount = triggerRetries.Count;
      var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
      return (triggerRetries, PaginationHelper);
  }

  public async Task<TriggerRetryBusinessModel> GetTriggerRetryAsync(int triggerRetryId)
  {
    TriggerRetry triggerRetry = await _context.TriggerRetries.AsNoTracking()
      .Where(tr => tr.Id == triggerRetryId)
      .Include(tr => tr.Trigger)
      .Include(tr => tr.AutoTask)
      .FirstOrDefaultAsync() ?? throw new LgdxNotFound404Exception();

    int SameTriggerFailed = await _context.TriggerRetries.AsNoTracking()
      .Where(tr => tr.TriggerId == triggerRetry.TriggerId)
      .CountAsync();

    return new TriggerRetryBusinessModel {
      Id = triggerRetry.Id,
      TriggerId = triggerRetry.TriggerId,
      TriggerName = triggerRetry.Trigger.Name,
      TriggerUrl = triggerRetry.Trigger.Url,
      TriggerHttpMethodId = triggerRetry.Trigger.HttpMethodId,
      AutoTaskId = triggerRetry.AutoTaskId,
      AutoTaskName = triggerRetry.AutoTask.Name,
      Body = triggerRetry.Body,
      SameTriggerFailed = SameTriggerFailed,
      CreatedAt = triggerRetry.CreatedAt
    };
  }

  public async Task<bool> DeleteTriggerRetryAsync(int triggerRetryId)
  {
    return await _context.TriggerRetries.Where(tr => tr.Id == triggerRetryId)
      .ExecuteDeleteAsync() >= 1;
  }

  public async Task RetryTriggerRetryAsync(int triggerRetryId)
  {
    var triggerRetry = await _context.TriggerRetries.AsNoTracking()
      .Where(tr => tr.Id == triggerRetryId)
      .FirstOrDefaultAsync() ?? throw new LgdxNotFound404Exception();

    var autoTask = await _context.AutoTasks.AsNoTracking()
      .Where(at => at.Id == triggerRetry.AutoTaskId)
      .FirstOrDefaultAsync() ?? throw new LgdxValidation400Expection(nameof(triggerRetry.AutoTaskId), "AutoTask ID is invalid.");

    var trigger = await _context.Triggers.AsNoTracking()
      .Where(t => t.Id == triggerRetry.TriggerId)
      .FirstOrDefaultAsync() ?? throw new LgdxValidation400Expection(nameof(triggerRetry.TriggerId), "Trigger ID is invalid.");

    if (await _triggerService.RetryTriggerAsync(autoTask, trigger, triggerRetry.Body))
    {
      await DeleteTriggerRetryAsync(triggerRetryId);
    }

    await _activityService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(TriggerRetry),
      EntityId = trigger.Id.ToString(),
      Action = ActivityAction.TriggerRetry,
    });
  }

  public async Task RetryAllFailedTriggerAsync(int triggerId)
  {
    var triggerRetries = await _context.TriggerRetries
      .Where(tr => tr.TriggerId == triggerId)
      .Include(tr => tr.Trigger)
      .Include(tr => tr.AutoTask)
      .AsSplitQuery()
      .ToListAsync();

    foreach (var tr in triggerRetries)
    {
      if (await _triggerService.RetryTriggerAsync(tr.AutoTask, tr.Trigger, tr.Body))
      {
        _context.TriggerRetries.Remove(tr);
      }
    }
    _context.SaveChanges();

    await _activityService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(TriggerRetry),
      EntityId = triggerId.ToString(),
      Action = ActivityAction.TriggerRetry,
      Note = $"Batch retry for {triggerRetries.Count} requests."
    });
  }
}