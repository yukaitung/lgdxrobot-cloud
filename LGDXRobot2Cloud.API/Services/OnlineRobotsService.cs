using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Protos;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.API.Services;

public record RobotDataComposite
{
  public required RobotClientsRobotCommands Commands { get; set; }
  public required RobotClientsExchange Data { get; set; }
  public bool UnresolvableCriticalStatus { get; set; } = false;
}

public interface IOnlineRobotsService
{
  Task AddRobotAsync(Guid robotId);
  Task RemoveRobotAsync(Guid robotId);
  Task SetRobotDataAsync(Guid robotId, RobotClientsExchange data);
  Task<Dictionary<Guid, RobotDataComposite>> GetRobotDataAsync(Guid robotId);
  Task<Dictionary<Guid, RobotDataComposite>> GetRobotsDataAsync(List<Guid> robotIds);
  Task<RobotClientsRobotCommands?> GetRobotCommands(Guid robotId);

  Task<bool> IsRobotOnlineAsync(Guid robotId);
  Task<bool> UpdateAbortTaskAsync(Guid robotId, bool enable);
  Task<bool> UpdateSoftwareEmergencyStopAsync(Guid robotId, bool enable);
  Task<bool> UpdatePauseTaskAssigementAsync(Guid robotId, bool enable);
  Task<bool> GetPauseAutoTaskAssignmentAsync(Guid robotId);

  void SetAutoTaskNext(Guid robotId, AutoTask task);
  AutoTask? GetAutoTaskNext(Guid robotId);
}

public class OnlineRobotsService(
    IBus bus,
    IDistributedCache cache, 
    IMemoryCache memoryCache
  ) : IOnlineRobotsService
{
  private readonly DistributedCacheEntryOptions _cacheEntryOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly string OnlineRobotssKey = "OnlineRobotsService_OnlineRobotss";

  static private bool GenerateUnresolvableCriticalStatus(RobotClientsRobotCriticalStatus criticalStatus)
  {
    if (criticalStatus.HardwareEmergencyStop ||
        criticalStatus.BatteryLow.Count > 0 ||
        criticalStatus.MotorDamaged.Count > 0) 
    {
      return true;
    }
    return false;
  }

  public async Task AddRobotAsync(Guid robotId)
  {
    var OnlineRobotsIds = await _cache.GetAsync<HashSet<Guid>>(OnlineRobotssKey) ?? [];
    OnlineRobotsIds.Add(robotId);
    // Online Robots
    await _cache.SetAsync(OnlineRobotssKey, OnlineRobotsIds, _cacheEntryOptions);
    // Robot Data
    await _cache.SetAsync($"OnlineRobotsService_RobotData_{robotId}", new RobotDataComposite{
      Commands = new RobotClientsRobotCommands(),
      Data = new RobotClientsExchange()
    }, _cacheEntryOptions);
  }

  public async Task RemoveRobotAsync(Guid robotId)
  {
    var OnlineRobotsIds = await _cache.GetAsync<HashSet<Guid>>(OnlineRobotssKey);
    if (OnlineRobotsIds != null && OnlineRobotsIds.Contains(robotId))
    {
      OnlineRobotsIds.Remove(robotId);
      await _cache.SetAsync(OnlineRobotssKey, OnlineRobotsIds, _cacheEntryOptions);
    }    
    await _cache.RemoveAsync($"OnlineRobotsService_RobotData_{robotId}");
  }

  public async Task SetRobotDataAsync(Guid robotId, RobotClientsExchange data)
  {
    await _bus.Publish(new RobotDataContract {
      RobotId = robotId,
      Position = new RobotDof {
        X = data.Position.X,
        Y = data.Position.Y,
        Rotation = data.Position.Rotation
      }
    });
    
    var robotDataComposite = await _cache.GetAsync<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}");
    if (robotDataComposite != null)
    {
      robotDataComposite.Data = data;
      robotDataComposite.UnresolvableCriticalStatus = GenerateUnresolvableCriticalStatus(robotDataComposite.Data.CriticalStatus);
      await _cache.SetAsync($"OnlineRobotsService_RobotData_{robotId}", robotDataComposite, _cacheEntryOptions);
    }
  }

  public async Task<Dictionary<Guid, RobotDataComposite>> GetRobotDataAsync(Guid robotId)
  {
    return await GetRobotsDataAsync([robotId]);
  }

  public async Task<Dictionary<Guid, RobotDataComposite>> GetRobotsDataAsync(List<Guid> robotIds)
  {
    Dictionary<Guid, RobotDataComposite> result = [];
    foreach (var robotId in robotIds)
    {
      var robotDataComposite = await _cache.GetAsync<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}");
      if (robotDataComposite != null)
      {
        result[robotId] = robotDataComposite;
      }
    }
    return result;
  }

  public async Task<RobotClientsRobotCommands?> GetRobotCommands(Guid robotId)
  {
    var robotDataComposite = await _cache.GetAsync<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}");
    if (robotDataComposite != null)
    {
      return robotDataComposite.Commands;
    }
    return null;
  }

  public async Task<bool> IsRobotOnlineAsync(Guid robotId)
  {
    var OnlineRobotsIds = await _cache.GetAsync<HashSet<Guid>>(OnlineRobotssKey);
    return OnlineRobotsIds != null && OnlineRobotsIds.Contains(robotId);
  }

  public async Task<bool> UpdateAbortTaskAsync(Guid robotId, bool enable)
  {
    var robotDataComposite = await _cache.GetAsync<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}");
    if (robotDataComposite != null) 
    {
      robotDataComposite.Commands.AbortTask = enable;
      await _cache.SetAsync($"OnlineRobotsService_RobotData_{robotId}", robotDataComposite, _cacheEntryOptions);
      return true;
    }
    else 
    {
      return false;
    }
  }

  public async Task<bool> UpdateSoftwareEmergencyStopAsync(Guid robotId, bool enable)
  {
    var robotDataComposite = await _cache.GetAsync<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}");
    if (robotDataComposite != null) 
    {
      robotDataComposite.Commands.SoftwareEmergencyStop = enable;
      await _cache.SetAsync($"OnlineRobotsService_RobotData_{robotId}", robotDataComposite, _cacheEntryOptions);
      return true;
    }
    else 
    {
      return false;
    }
  }

  public async Task<bool> UpdatePauseTaskAssigementAsync(Guid robotId, bool enable)
  {
    var robotDataComposite = await _cache.GetAsync<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}");
    if (robotDataComposite != null) 
    {
      robotDataComposite.Commands.PauseTaskAssigement = enable;
      await _cache.SetAsync($"OnlineRobotsService_RobotData_{robotId}", robotDataComposite, _cacheEntryOptions);
      return true;
    }
    else 
    {
      return false;
    }
  }

  public async Task<bool> GetPauseAutoTaskAssignmentAsync(Guid robotId)
  {
    var robotDataComposite = await _cache.GetAsync<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}");
    if (robotDataComposite != null) 
    {
      return robotDataComposite.Commands.PauseTaskAssigement;
    }
    return false;
  }

  public void SetAutoTaskNext(Guid robotId, AutoTask autoTask)
  {
    _memoryCache.Set($"OnlineRobotsService_RobotHasNextTask_{robotId}", autoTask);
  }

  public AutoTask? GetAutoTaskNext(Guid robotId)
  {
    if (_memoryCache.TryGetValue($"OnlineRobotsService_RobotHasNextTask_{robotId}", out AutoTask? autoTask))
    {
      _memoryCache.Remove($"OnlineRobotsService_RobotHasNextTask_{robotId}");
      return autoTask;
    }
    else
    {
      return null;
    }
  }
}