using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Services;
using MassTransit;

namespace LGDXRobot2Cloud.UI.Consumers;

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