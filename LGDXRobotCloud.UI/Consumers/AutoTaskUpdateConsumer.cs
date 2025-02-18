using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.UI.Services;
using MassTransit;

namespace LGDXRobotCloud.UI.Consumers;

public class AutoTaskUpdateConsumer(
    IRealTimeService realTimeService
  ) : IConsumer<AutoTaskUpdateContract>
{
  private readonly IRealTimeService _realTimeService = realTimeService ?? throw new ArgumentNullException(nameof(realTimeService));
  public Task Consume(ConsumeContext<AutoTaskUpdateContract> context)
  {
    _realTimeService.AutoTaskHasUpdated(new AutoTaskUpdatEventArgs { AutoTaskUpdateContract = context.Message });
    return Task.CompletedTask;
  }
}