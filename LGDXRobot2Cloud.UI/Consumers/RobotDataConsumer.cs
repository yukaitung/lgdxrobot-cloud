using LGDXRobot2Cloud.Data.Contracts;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.UI.Consumers;

public class RobotDataConsumer(
    IMemoryCache memoryCache
  ) : IConsumer<RobotDataContract>
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  public Task Consume(ConsumeContext<RobotDataContract> context)
  {
    if (!_memoryCache.TryGetValue($"RobotData_{context.Message.RobotId}", out var _))
    {
      // Register the robot data in the cache
      var OnlineRobotsIds = _memoryCache.Get<HashSet<Guid>>("OnlineRobots") ?? [];
      OnlineRobotsIds.Add(context.Message.RobotId);
      memoryCache.Set("OnlineRobots", OnlineRobotsIds);
    }
    _memoryCache.Set($"RobotData_{context.Message.RobotId}", context.Message, DateTimeOffset.Now.AddMinutes(1));
    return Task.CompletedTask;
  }
}