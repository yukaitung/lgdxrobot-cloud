using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.UI.Services;
using MassTransit;

namespace LGDXRobotCloud.UI.Consumers;

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