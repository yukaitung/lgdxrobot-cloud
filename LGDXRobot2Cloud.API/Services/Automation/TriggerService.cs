using System.Text.Json;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Services.Automation;

public interface ITriggerService
{
  Task InitialiseTriggerAsync(AutoTask autoTask, FlowDetail flowDetail, Trigger trigger);
  Task RetryTriggerAsync(AutoTask autoTask, Trigger trigger, string body);
}

public sealed class TriggerService (
  IBus bus,
  LgdxContext context
) : ITriggerService
{
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

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
      (int)TriggerPresetValue.AutoTaskName => $"\"{autoTask.Name}\"",
      (int)TriggerPresetValue.AutoTaskCurrentProgressId => $"{autoTask.CurrentProgressId}",
      (int)TriggerPresetValue.AutoTaskCurrentProgressName => $"\"{autoTask.CurrentProgress.Name!}\"",
      (int)TriggerPresetValue.RobotId => $"\"{autoTask.AssignedRobotId}\"",
      (int)TriggerPresetValue.RobotName => $"\"{GetRobotName((Guid)autoTask.AssignedRobotId!)}\"",
      (int)TriggerPresetValue.RealmId => $"\"{autoTask.RealmId}\"",
      (int)TriggerPresetValue.RealmName => $"\"{GetRealmName(autoTask.RealmId)}\"",
      _ => string.Empty,
    };
  }

  public async Task InitialiseTriggerAsync(AutoTask autoTask, FlowDetail flowDetail, Trigger trigger)
  {
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
      if (flowDetail.AutoTaskNextControllerId != (int) AutoTaskNextController.Robot && autoTask.NextToken != null)
      {
        bodyDictionary.Add("NextToken", autoTask.NextToken);
      }

      await _bus.Publish(new AutoTaskTriggerContract {
        Trigger = trigger,
        Body = bodyDictionary,
        AutoTaskId = autoTask.Id,
        AutoTaskName = autoTask.Name!,
        RobotId = (Guid)autoTask.AssignedRobotId!,
        RobotName = GetRobotName((Guid)autoTask.AssignedRobotId!),
        RealmId = autoTask.RealmId,
        RealmName = GetRealmName(autoTask.RealmId),
      });
    }
  }

  public async Task RetryTriggerAsync(AutoTask autoTask, Trigger trigger, string body)
  {
    var bodyDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(body ?? "{}");
    if (bodyDictionary != null)
    {
      await _bus.Publish(new AutoTaskTriggerContract {
        Trigger = trigger,
        Body = bodyDictionary,
        AutoTaskId = autoTask.Id,
        AutoTaskName = autoTask.Name!,
        RobotId = (Guid)autoTask.AssignedRobotId!,
        RobotName = GetRobotName((Guid)autoTask.AssignedRobotId!),
        RealmId = autoTask.RealmId,
        RealmName = GetRealmName(autoTask.RealmId),
      });
    }
  }
}