using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.UI.Services;
using MassTransit;

namespace LGDXRobotCloud.UI.Consumers;

public class SlamDataConsumer(ISlamService slamService) : IConsumer<SlamDataContract>
{
  private readonly ISlamService _slamService = slamService ?? throw new ArgumentNullException(nameof(slamService));

  public Task Consume(ConsumeContext<SlamDataContract> context)
  {
    _slamService.UpdateSlamData(context.Message);
    return Task.CompletedTask;
  }
}