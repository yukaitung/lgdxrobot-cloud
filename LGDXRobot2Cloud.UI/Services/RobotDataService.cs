using LGDXRobot2Cloud.Data.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRobotDataService
{
  RobotDataContract? GetRobotData(Guid robotId);
}

public sealed class RobotDataService(
    IMemoryCache memoryCache
  ) : IRobotDataService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  public RobotDataContract? GetRobotData(Guid robotId)
  {
    if (_memoryCache.TryGetValue("RobotDataConsumer_RobotsData", out Dictionary<Guid, RobotDataContract>? robotsData))
    {
      if (robotsData != null && robotsData.TryGetValue(robotId, out var robotData))
      {
        return robotData;
      }
    }
    return null;
  }
}