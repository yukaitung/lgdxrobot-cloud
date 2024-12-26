using LGDXRobot2Cloud.Data.Contracts;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.UI.Consumers;

public class RobotCommandsConsumer(
  IMemoryCache memoryCache
  ) : IConsumer<RobotCommandsContract>
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  public Task Consume(ConsumeContext<RobotCommandsContract> context)
  {
    _memoryCache.Set($"RobotCommands_{context.Message.RobotId}", context.Message, DateTimeOffset.Now.AddMinutes(1));
    return Task.CompletedTask;
  }
}