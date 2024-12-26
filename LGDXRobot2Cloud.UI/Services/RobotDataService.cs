using LGDXRobot2Cloud.Data.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRobotDataService
{
  HashSet<Guid> GetOnlineRobots();
  RobotDataContract? GetRobotData(Guid robotId);
  RobotCommandsContract? GetRobotCommands(Guid robotId);
}

public sealed class RobotDataService(
    IMemoryCache memoryCache
  ) : IRobotDataService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  public HashSet<Guid> GetOnlineRobots()
  {
    return _memoryCache.Get<HashSet<Guid>>("OnlineRobots") ?? [];
  }

  public RobotDataContract? GetRobotData(Guid robotId)
  {
    if (_memoryCache.TryGetValue($"RobotData_{robotId}", out RobotDataContract? robotData))
    {
      return robotData;
    }
    else
    {
      // Unregister the robot data in the cache
      var OnlineRobotsIds = _memoryCache.Get<HashSet<Guid>>("OnlineRobots") ?? [];
      OnlineRobotsIds.Remove(robotId);
      _memoryCache.Set("OnlineRobots", OnlineRobotsIds);
      return null;
    }
  }

  public RobotCommandsContract? GetRobotCommands(Guid robotId)
  {
    if (_memoryCache.TryGetValue($"RobotCommands_{robotId}", out RobotCommandsContract? robotCommands))
    {
      return robotCommands;
    }
    return null;
  }
}