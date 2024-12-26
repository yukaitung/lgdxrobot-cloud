using LGDXRobot2Cloud.Data.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRobotDataService
{
  RobotDataContract? GetRobotData(Guid robotId);
  RobotCommandsContract? GetRobotCommands(Guid robotId);
}

public sealed class RobotDataService(
    IMemoryCache memoryCache
  ) : IRobotDataService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  public RobotDataContract? GetRobotData(Guid robotId)
  {
    if (_memoryCache.TryGetValue($"RobotData_{robotId}", out RobotDataContract? robotData))
    {
      return robotData;
    }
    return null;
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