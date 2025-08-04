using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.UI.Services;
using MassTransit;

namespace LGDXRobotCloud.UI.Consumers;

public class SlamMapDataConsumer(ISlamService slamService) : IConsumer<SlamMapDataContract>
{
  private readonly ISlamService _slamService = slamService ?? throw new ArgumentNullException(nameof(slamService));

  public Task Consume(ConsumeContext<SlamMapDataContract> context)
  {
    _slamService.UpdateSlamData(context.Message);
    return Task.CompletedTask;
  }
}