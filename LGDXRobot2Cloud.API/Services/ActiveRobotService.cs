using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.API.Services;

public interface IActiveRobotService
{
  void AddRobot(Guid robotId);
  void RemoveRobot(Guid robotId);
  HashSet<Guid> GetConnectedRobots();
  void SetRobotData(Guid robotId, RobotClientsExchange data);

  bool IsRobotActive(Guid robotId);
  Dictionary<Guid, RobotStatus> GetRobotsStatus();
}

public class ActiveRobotService(IMemoryCache memoryCache) : IActiveRobotService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  private readonly string ActiveRobotsKey = "ActiveRobotService_ActiveRobots";
  private readonly string ActiveRobotsStatusKey = "ActiveRobotService_ActiveRobotsStatus";

  private RobotStatus ConvertRobotStatus(RobotClientsRobotStatus robotStatus)
  {
    switch (robotStatus)
    {
      case RobotClientsRobotStatus.Idle:
        return RobotStatus.Idle;
      case RobotClientsRobotStatus.Running:
        return RobotStatus.Running;
      case RobotClientsRobotStatus.Stuck:
        return RobotStatus.Stuck;
      case RobotClientsRobotStatus.Aborting:
        return RobotStatus.Aborting;
      case RobotClientsRobotStatus.Paused:
        return RobotStatus.Paused;
      case RobotClientsRobotStatus.Critical:
        return RobotStatus.Critical;
      case RobotClientsRobotStatus.Charging:
        return RobotStatus.Charging;
      case RobotClientsRobotStatus.Offline:
        return RobotStatus.Offline;
    }
    return RobotStatus.Offline;
  }

  public void AddRobot(Guid robotId)
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(ActiveRobotsKey, out var activeRobotIds);
    activeRobotIds ??= [];
    activeRobotIds.Add(robotId);
    _memoryCache.Set(ActiveRobotsKey, activeRobotIds, TimeSpan.FromDays(1));
  }

  public void RemoveRobot(Guid robotId)
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(ActiveRobotsKey, out var activeRobotIds);
    if (activeRobotIds != null && activeRobotIds.Contains(robotId))
    {
      activeRobotIds.Remove(robotId);
      _memoryCache.Set(ActiveRobotsKey, activeRobotIds, TimeSpan.FromDays(1));
    }    
    _memoryCache.Remove($"ActiveRobotService_RobotData_{robotId}");
    _memoryCache.TryGetValue<Dictionary<Guid, RobotStatus>>(ActiveRobotsStatusKey, out var activeRobotsStatus);
    if (activeRobotsStatus != null && activeRobotsStatus.ContainsKey(robotId))
    {
      activeRobotsStatus.Remove(robotId);
      _memoryCache.Set(ActiveRobotsStatusKey, activeRobotsStatus, TimeSpan.FromDays(1));
    }
  }

  public bool IsRobotActive(Guid robotId)
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(ActiveRobotsKey, out var activeRobotIds);
    if (activeRobotIds != null && activeRobotIds.Contains(robotId))
    {
      return true;
    }
    else 
    {
      return false;
    }
  }

  public HashSet<Guid> GetConnectedRobots()
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(ActiveRobotsKey, out var activeRobotIds);
    return activeRobotIds ?? [];
  }

  public void SetRobotData(Guid robotId, RobotClientsExchange data)
  {
    _memoryCache.Set($"ActiveRobotService_RobotData_{robotId}", data, TimeSpan.FromMinutes(5));
    _memoryCache.TryGetValue<Dictionary<Guid, RobotStatus>>(ActiveRobotsStatusKey, out var activeRobotsStatus);
    activeRobotsStatus ??= [];
    activeRobotsStatus[robotId] = ConvertRobotStatus(data.RobotStatus);
    _memoryCache.Set(ActiveRobotsStatusKey, activeRobotsStatus, TimeSpan.FromMinutes(5));
  }

  public Dictionary<Guid, RobotStatus> GetRobotsStatus()
  {
    _memoryCache.TryGetValue<Dictionary<Guid, RobotStatus>>(ActiveRobotsStatusKey, out var activeRobotsStatus);
    return activeRobotsStatus ?? [];
  }
}