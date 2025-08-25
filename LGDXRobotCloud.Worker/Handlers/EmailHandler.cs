using LGDXRobotCloud.Data.Models.RabbitMQ;
using LGDXRobotCloud.Worker.Services;

namespace LGDXRobotCloud.Worker.Handlers;

public class EmailHandler(IEmailService emailService)
{
  private readonly IEmailService _emailService = emailService;

  public async ValueTask Handle(EmailRequest emailRequest)
  {
    await _emailService.SendEmailAsync(emailRequest);
  }
}