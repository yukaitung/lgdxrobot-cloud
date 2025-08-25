using System.Text.Json;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Data.Models.RabbitMQ;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace LGDXRobotCloud.API.Services.Automation;

public interface ITriggerService
{
  Task<(IEnumerable<TriggerListBusinessModel>, PaginationHelper)> GetTriggersAsync(string? name, int pageNumber, int pageSize);
  Task<TriggerBusinessModel> GetTriggerAsync(int triggerId);
  Task<TriggerBusinessModel> CreateTriggerAsync(TriggerCreateBusinessModel triggerCreateBusinessModel);
  Task<bool> UpdateTriggerAsync(int triggerId, TriggerUpdateBusinessModel triggerUpdateBusinessModel);
  Task<bool> TestDeleteTriggerAsync(int triggerId);
  Task<bool> DeleteTriggerAsync(int triggerId);

  Task<IEnumerable<TriggerSearchBusinessModel>> SearchTriggersAsync(string? name);

  Task InitialiseTriggerAsync(AutoTask autoTask, FlowDetail flowDetail);
  Task<bool> RetryTriggerAsync(AutoTask autoTask, Trigger trigger, string body);
}

public sealed class TriggerService (
  IActivityLogService activityService,
  IApiKeyService apiKeyService,
  IMessageBus bus,
  LgdxContext context
) : ITriggerService
{
  private readonly IActivityLogService _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
  private readonly IApiKeyService _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
  private readonly IMessageBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<TriggerListBusinessModel>, PaginationHelper)> GetTriggersAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Triggers as IQueryable<Trigger>;
    if(!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(t => t.Name.ToLower().Contains(name.ToLower()));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var triggers = await query.AsNoTracking()
      .OrderBy(t => t.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(t => new TriggerListBusinessModel {
        Id = t.Id,
        Name = t.Name,
        Url = t.Url,
        HttpMethodId = t.HttpMethodId,
      })
      .ToListAsync();
    return (triggers, PaginationHelper);
  }

  public async Task<TriggerBusinessModel> GetTriggerAsync(int triggerId)
  {
    return await _context.Triggers.Where(t => t.Id == triggerId)
      .Include(t => t.ApiKey)
      .Select(t => new TriggerBusinessModel {
        Id = t.Id,
        Name = t.Name,
        Url = t.Url,
        HttpMethodId = t.HttpMethodId,
        Body = t.Body,
        ApiKeyInsertLocationId = t.ApiKeyInsertLocationId,
        ApiKeyFieldName = t.ApiKeyFieldName,
        ApiKeyId = t.ApiKey!.Id,
        ApiKeyName = t.ApiKey!.Name,
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  private async Task ValidateApiKey(int apiKeyId)
  {
    var apiKey = await _apiKeyService.GetApiKeyAsync(apiKeyId);
    if (apiKey == null)
    {
      throw new LgdxValidation400Expection(nameof(TriggerBusinessModel.Id), $"The API Key Id {apiKeyId} is invalid.");
    }
    else if (!apiKey.IsThirdParty)
    {
      throw new LgdxValidation400Expection(nameof(TriggerBusinessModel.Id), "Only third party API key is allowed.");
    }
  }

  public async Task<TriggerBusinessModel> CreateTriggerAsync(TriggerCreateBusinessModel triggerCreateBusinessModel)
  {
    if (triggerCreateBusinessModel.ApiKeyId != null)
    {
      await ValidateApiKey((int)triggerCreateBusinessModel.ApiKeyId);
    }

    var trigger = new Trigger {
      Name = triggerCreateBusinessModel.Name,
      Url = triggerCreateBusinessModel.Url,
      HttpMethodId = triggerCreateBusinessModel.HttpMethodId,
      Body = triggerCreateBusinessModel.Body,
      ApiKeyInsertLocationId = triggerCreateBusinessModel.ApiKeyInsertLocationId,
      ApiKeyFieldName = triggerCreateBusinessModel.ApiKeyFieldName,
      ApiKeyId = triggerCreateBusinessModel.ApiKeyId,
    };
    await _context.Triggers.AddAsync(trigger);
    await _context.SaveChangesAsync();
    
    await _activityService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(Trigger),
      EntityId = trigger.Id.ToString(),
      Action = ActivityAction.Create,
    });

    return new TriggerBusinessModel
    {
      Id = trigger.Id,
      Name = trigger.Name,
      Url = trigger.Url,
      HttpMethodId = trigger.HttpMethodId,
      Body = trigger.Body,
      ApiKeyInsertLocationId = trigger.ApiKeyInsertLocationId,
      ApiKeyFieldName = trigger.ApiKeyFieldName,
      ApiKeyId = trigger.ApiKeyId,
      ApiKeyName = trigger.ApiKey?.Name,
    };
  }

  public async Task<bool> UpdateTriggerAsync(int triggerId, TriggerUpdateBusinessModel triggerUpdateBusinessModel)
  {
    if (triggerUpdateBusinessModel.ApiKeyId != null)
    {
      await ValidateApiKey((int)triggerUpdateBusinessModel.ApiKeyId);
    }

    bool result = await _context.Triggers
      .Where(t => t.Id == triggerId)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(t => t.Name, triggerUpdateBusinessModel.Name)
        .SetProperty(t => t.Url, triggerUpdateBusinessModel.Url)
        .SetProperty(t => t.HttpMethodId, triggerUpdateBusinessModel.HttpMethodId)
        .SetProperty(t => t.Body, triggerUpdateBusinessModel.Body)
        .SetProperty(t => t.ApiKeyInsertLocationId, triggerUpdateBusinessModel.ApiKeyInsertLocationId)
        .SetProperty(t => t.ApiKeyFieldName, triggerUpdateBusinessModel.ApiKeyFieldName)
        .SetProperty(t => t.ApiKeyId, triggerUpdateBusinessModel.ApiKeyId)) == 1;
    
    if (result)
    {
      await _activityService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Trigger),
        EntityId = triggerId.ToString(),
        Action = ActivityAction.Update,
      });
    }
    return result;
  }

  public async Task<bool> TestDeleteTriggerAsync(int triggerId)
  {
    var depeendencies = await _context.FlowDetails.Where(t => t.TriggerId == triggerId).CountAsync();
    if (depeendencies > 0)
    {
      throw new LgdxValidation400Expection(nameof(triggerId), $"This trigger has been used by {depeendencies} details in flows.");
    }
    return true;
  }

  public async Task<bool> DeleteTriggerAsync(int triggerId)
  {
    bool result = await _context.Triggers.Where(t => t.Id == triggerId)
      .ExecuteDeleteAsync() == 1;

    if (result)
    {
      await _activityService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Trigger),
        EntityId = triggerId.ToString(),
        Action = ActivityAction.Delete,
      });
    }
    return result;
  }

  public async Task<IEnumerable<TriggerSearchBusinessModel>> SearchTriggersAsync(string? name)
  {
    var n = name ?? string.Empty;
    return await _context.Triggers.AsNoTracking()
      .Where(w => w.Name.ToLower().Contains(n.ToLower()))
      .Take(10)
      .Select(t => new TriggerSearchBusinessModel {
        Id = t.Id,
        Name = t.Name,
      })
      .ToListAsync();
  }

  private string GetRobotName(Guid robotId)
  {
    return _context.Robots.AsNoTracking().Where(r => r.Id == robotId).FirstOrDefault()?.Name ?? string.Empty;
  }

  private string GetRealmName(int realmId)
  {
    return _context.Realms.AsNoTracking().Where(r => r.Id == realmId).FirstOrDefault()?.Name ?? string.Empty;
  }

  private string GeneratePresetValue(int i, AutoTask autoTask)
  {
    return i switch
    {
      (int)TriggerPresetValue.AutoTaskId => $"{autoTask.Id}",
      (int)TriggerPresetValue.AutoTaskName => $"{autoTask.Name}",
      (int)TriggerPresetValue.AutoTaskCurrentProgressId => $"{autoTask.CurrentProgressId}",
      (int)TriggerPresetValue.AutoTaskCurrentProgressName => $"{autoTask.CurrentProgress.Name!}",
      (int)TriggerPresetValue.RobotId => $"{autoTask.AssignedRobotId}",
      (int)TriggerPresetValue.RobotName => $"{GetRobotName((Guid)autoTask.AssignedRobotId!)}",
      (int)TriggerPresetValue.RealmId => $"{autoTask.RealmId}",
      (int)TriggerPresetValue.RealmName => $"{GetRealmName(autoTask.RealmId)}",
      _ => string.Empty,
    };
  }

  public async Task InitialiseTriggerAsync(AutoTask autoTask, FlowDetail flowDetail)
  {
    var trigger = await _context.Triggers.AsNoTracking()
      .Where(t => t.Id == flowDetail.TriggerId)
      .FirstOrDefaultAsync();
    if (trigger == null)
    {
      return;
    }
    
    var bodyDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(trigger.Body ?? "{}");
    if (bodyDictionary != null)
    {
      // Replace Preset Value
      foreach (var pair in bodyDictionary)
      {
        if (pair.Value.Length >= 5) // ((1)) has 5 characters
        {
          if (int.TryParse(pair.Value[2..^2], out int p))
          {
            bodyDictionary[pair.Key] = GeneratePresetValue(p, autoTask);
          }
        }
      }
      // Add Next Token
      if (flowDetail.AutoTaskNextControllerId != (int)AutoTaskNextController.Robot && autoTask.NextToken != null)
      {
        bodyDictionary.Add("NextToken", autoTask.NextToken);
      }

      await _bus.PublishAsync(new AutoTaskTriggerContract
      {
        Trigger = trigger,
        Body = bodyDictionary,
        AutoTaskId = autoTask.Id,
        AutoTaskName = autoTask.Name!,
        RobotId = (Guid)autoTask.AssignedRobotId!,
        RobotName = GetRobotName((Guid)autoTask.AssignedRobotId!),
        RealmId = autoTask.RealmId,
        RealmName = GetRealmName(autoTask.RealmId),
      });
      
      await _activityService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Trigger),
        EntityId = trigger.Id.ToString(),
        Action = ActivityAction.TriggerExecute,
      });
    }
  }

  public async Task<bool> RetryTriggerAsync(AutoTask autoTask, Trigger trigger, string body)
  {
    var bodyDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(body ?? "{}");
    if (bodyDictionary != null)
    {
      await _bus.PublishAsync(new AutoTaskTriggerContract {
        Trigger = trigger,
        Body = bodyDictionary,
        AutoTaskId = autoTask.Id,
        AutoTaskName = autoTask.Name!,
        RobotId = (Guid)autoTask.AssignedRobotId!,
        RobotName = GetRobotName((Guid)autoTask.AssignedRobotId!),
        RealmId = autoTask.RealmId,
        RealmName = GetRealmName(autoTask.RealmId),
      });
      // Don't add activity log
      return true;
    }
    return false;
  }
}