using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobotCloud.UI.Services;

public interface IRobotDataService
{
  HashSet<Guid> GetOnlineRobots(int realmId);

  RobotDataContract? GetRobotData(Guid robotId, int realmId);
  void UpdateRobotData(RobotDataContract robotData);
}

public sealed class RobotDataService(
    IMemoryCache memoryCache,
    IRealTimeService realTimeService
  ) : IRobotDataService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IRealTimeService _realTimeService = realTimeService ?? throw new ArgumentNullException(nameof(realTimeService));
  private static string GetRobotDataKey(Guid robotId) => $"RobotDataService_RobotData_{robotId}";
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
    return null;
  }

  public void UpdateRobotData(RobotDataContract robotData)
  {
    var robotId = robotData.RobotId;
    var realmId = robotData.RealmId;
    if (!_memoryCache.TryGetValue(GetRobotDataKey(robotId), out var _) && robotData.RobotStatus != RobotStatus.Offline)
    {
      // Register the robot data in the cache
      var OnlineRobotsIds = _memoryCache.Get<HashSet<Guid>>(GetOnlineRobotsKey(realmId)) ?? [];
      OnlineRobotsIds.Add(robotId);
      memoryCache.Set(GetOnlineRobotsKey(realmId), OnlineRobotsIds);
    }
    else if (robotData.RobotStatus == RobotStatus.Offline)
    {
      // Remove the robot data from the cache
      var OnlineRobotsIds = _memoryCache.Get<HashSet<Guid>>(GetOnlineRobotsKey(realmId)) ?? [];
      OnlineRobotsIds.Remove(robotId);
      memoryCache.Set(GetOnlineRobotsKey(realmId), OnlineRobotsIds);
    }
    _memoryCache.Set(GetRobotDataKey(robotId), robotData);
    _realTimeService.RobotDataHasUpdated(new RobotUpdatEventArgs { RobotId = robotId, RealmId = realmId });
  }
}