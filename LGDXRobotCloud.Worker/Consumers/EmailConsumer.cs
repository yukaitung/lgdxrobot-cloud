using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Worker.Services;
using MassTransit;

namespace LGDXRobotCloud.Worker.Consumers;

public class EmailConsumer(IEmailService emailService) : IConsumer<EmailContract>
{
  private readonly IEmailService _emailService = emailService;

  public async Task Consume(ConsumeContext<EmailContract> context)
  {
    await _emailService.SendEmailAsync(context.Message);
  }
}