using MassTransit;
using LGDXRobot2Cloud.Data.Contracts;

namespace LGDXRobot2Cloud.Worker.Consumers;

public class BusTestConsumer : IConsumer<BusTest>
{
  public Task Consume(ConsumeContext<BusTest> context)
  {
    Console.WriteLine(context.Message.Value);
    return Task.CompletedTask;
  }
}
