using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Worker.Services;
using MassTransit;

namespace LGDXRobot2Cloud.Worker.Consumers;

public class EmailConsumer(IEmailService emailService) : IConsumer<EmailContract>
{
  private readonly IEmailService _emailService = emailService;

  public async Task Consume(ConsumeContext<EmailContract> context)
  {
    await _emailService.SendEmailAsync(context.Message);
  }
}