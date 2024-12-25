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
    var robotsData = _memoryCache.Get<Dictionary<Guid, RobotDataContract>>("RobotDataConsumer_RobotsData") ?? [];
    robotsData[context.Message.RobotId] = context.Message;
    _memoryCache.Set($"RobotDataConsumer_RobotsData", robotsData);
    return Task.CompletedTask;
  }
}