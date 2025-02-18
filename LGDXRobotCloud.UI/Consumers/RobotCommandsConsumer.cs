using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.UI.Services;
using MassTransit;

namespace LGDXRobotCloud.UI.Consumers;

public class RobotCommandsConsumer(
    IRobotDataService robotDataService
  ) : IConsumer<RobotCommandsContract>
{
  private readonly IRobotDataService _robotDataService = robotDataService ?? throw new ArgumentNullException(nameof(robotDataService));

  public Task Consume(ConsumeContext<RobotCommandsContract> context)
  {
    _robotDataService.UpdateRobotCommands(context.Message);
    return Task.CompletedTask;
  }
}