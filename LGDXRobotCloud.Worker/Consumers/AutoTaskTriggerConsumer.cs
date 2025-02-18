using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Worker.Services;
using MassTransit;
namespace LGDXRobotCloud.Worker.Consumers;

public class AutoTaskTriggerConsumer(ITriggerService triggerService) : IConsumer<AutoTaskTriggerContract>
{
  private readonly ITriggerService _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));

  public async Task Consume(ConsumeContext<AutoTaskTriggerContract> context)
  {
    await _triggerService.TriggerAsync(context.Message);
  }
}