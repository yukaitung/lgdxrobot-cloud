using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Services;
using MassTransit;

namespace LGDXRobot2Cloud.UI.Consumers;

public class RobotDataConsumer(
    IRobotDataService robotDataService
  ) : IConsumer<RobotDataContract>
{
  private readonly IRobotDataService _robotDataService = robotDataService ?? throw new ArgumentNullException(nameof(robotDataService));

  public Task Consume(ConsumeContext<RobotDataContract> context)
  {
    _robotDataService.UpdateRobotData(context.Message);
    return Task.CompletedTask;
  }
}