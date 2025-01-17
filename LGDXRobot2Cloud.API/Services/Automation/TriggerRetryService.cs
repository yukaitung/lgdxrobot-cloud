using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Models.Business.Automation;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Services.Automation;

public interface ITriggerRetryService
{
  Task<(IEnumerable<TriggerRetryListBusinessModel>, PaginationHelper)> GetTriggerRetriesAsync(int pageNumber, int pageSize);
  Task<TriggerRetryBusinessModel> GetTriggerRetryAsync(int triggerRetryId);
  Task<bool> DeleteTriggerRetryAsync(int triggerRetryId);
  Task RetryTriggerRetryAsync(int triggerRetryId);
}

public class TriggerRetryService (
  ITriggerService triggerService,
  LgdxContext context
) : ITriggerRetryService
{
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
    return await _context.TriggerRetries.AsNoTracking()
      .Where(tr => tr.Id == triggerRetryId)
      .Include(tr => tr.Trigger)
      .Include(tr => tr.AutoTask)
      .Select(tr => new TriggerRetryBusinessModel {
        Id = tr.Id,
        TriggerId = tr.TriggerId,
        TriggerName = tr.Trigger.Name,
        TriggerUrl = tr.Trigger.Url,
        TriggerHttpMethodId = tr.Trigger.HttpMethodId,
        AutoTaskId = tr.AutoTaskId,
        AutoTaskName = tr.AutoTask.Name,
        Body = tr.Body,
        CreatedAt = tr.CreatedAt
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
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

    await _triggerService.RetryTriggerAsync(autoTask, trigger, triggerRetry.Body);

    if (!await DeleteTriggerRetryAsync(triggerRetryId))
    {
      throw new LgdxBusiness500Exception("Failed to delete trigger retry.");
    }
  }
}