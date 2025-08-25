using LGDXRobotCloud.Data.Models.RabbitMQ;
using LGDXRobotCloud.Worker.Services;

namespace LGDXRobotCloud.Worker.Handlers;

public class AutoTaskTriggerHandler(ITriggerService triggerService)
{
  private readonly ITriggerService _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));

  public async Task Handle(AutoTaskTriggerRequest autoTaskTriggerRequest)
  {
    await _triggerService.TriggerAsync(autoTaskTriggerRequest);
  }
}