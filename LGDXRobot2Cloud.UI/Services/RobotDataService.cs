using LGDXRobot2Cloud.Data.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRobotDataService
{
  HashSet<Guid> GetOnlineRobots(int realmId);

  RobotDataContract? GetRobotData(Guid robotId, int realmId);
  void UpdateRobotData(RobotDataContract robotData);

  RobotCommandsContract? GetRobotCommands(Guid robotId);
  void UpdateRobotCommands(RobotCommandsContract robotCommands);
}

public sealed class RobotDataService(
    IMemoryCache memoryCache,
    IRealTimeService realTimeService
  ) : IRobotDataService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IRealTimeService _realTimeService = realTimeService ?? throw new ArgumentNullException(nameof(realTimeService));
  private static string GetRobotDataKey(Guid robotId) => $"RobotDataService_RobotData_{robotId}";
  private static string GetRobotCommandsKey(Guid robotId) => $"RobotDataService_RobotCommands_{robotId}";
  private static string GetOnlineRobotsKey(int realmId) => $"RobotDataService_OnlineRobots_{realmId}";

  public HashSet<Guid> GetOnlineRobots(int realmId)
  {
    return _memoryCache.Get<HashSet<Guid>>(GetOnlineRobotsKey(realmId)) ?? [];
  }

  public RobotDataContract? GetRobotData(Guid robotId, int realmId)
  {
    if (_memoryCache.TryGetValue(GetRobotDataKey(robotId), out RobotDataContract? robotData))
    {
      return robotData;
    }
    else
    {
      // Unregister the robot data in the cache
      var OnlineRobotsIds = _memoryCache.Get<HashSet<Guid>>(GetOnlineRobotsKey(realmId)) ?? [];
      OnlineRobotsIds.Remove(robotId);
      _memoryCache.Set(GetOnlineRobotsKey(realmId), OnlineRobotsIds);
      return null;
    }
  }

  public void UpdateRobotData(RobotDataContract robotData)
  {
    var robotId = robotData.RobotId;
    var realmId = robotData.RealmId;
    if (!_memoryCache.TryGetValue(GetRobotDataKey(robotId), out var _))
    {
      // Register the robot data in the cache
      var OnlineRobotsIds = _memoryCache.Get<HashSet<Guid>>(GetOnlineRobotsKey(realmId)) ?? [];
      OnlineRobotsIds.Add(robotId);
      memoryCache.Set(GetOnlineRobotsKey(realmId), OnlineRobotsIds);
    }
    _memoryCache.Set(GetRobotDataKey(robotId), robotData, DateTimeOffset.Now.AddMinutes(1));
    _realTimeService.RobotDataHasUpdated(new RobotUpdatEventArgs { RobotId = robotId, RealmId = realmId });
  }

  public RobotCommandsContract? GetRobotCommands(Guid robotId)
  {
    if (_memoryCache.TryGetValue(GetRobotCommandsKey(robotId), out RobotCommandsContract? robotCommands))
    {
      return robotCommands;
    }
    return null;
  }

  public void UpdateRobotCommands(RobotCommandsContract robotCommands)
  {
    _memoryCache.Set(GetRobotCommandsKey(robotCommands.RobotId), robotCommands, DateTimeOffset.Now.AddMinutes(1));
    _realTimeService.RobotCommandsHasUpdated(new RobotUpdatEventArgs { RobotId = robotCommands.RobotId, RealmId = 0 });
  }
}