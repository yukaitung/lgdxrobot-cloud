using LGDXRobotCloud.Data.Models.RabbitMQ;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Worker.Configurations;
using LGDXRobotCloud.Worker.Strategies.Email;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LGDXRobotCloud.Worker.Services;

public interface IEmailService
{
  Task SendEmailAsync(EmailRequest emailContract);
}

public sealed class EmailService (
    IActivityLogService activityLogService,
    IOptionsSnapshot<EmailConfiguration> emailConfiguration,
    IOptionsSnapshot<EmailLinksConfiguration> emailLinksConfiguration,
    HtmlRenderer htmlRenderer
  ) : IEmailService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly EmailConfiguration _emailConfiguration = emailConfiguration.Value ?? throw new ArgumentNullException(nameof(emailConfiguration));
  private readonly EmailLinksConfiguration _emailLinksConfiguration = emailLinksConfiguration.Value ?? throw new ArgumentNullException(nameof(_emailLinksConfiguration));
  private readonly HtmlRenderer _htmlRenderer = htmlRenderer ?? throw new ArgumentNullException(nameof(htmlRenderer));

  private IEmailStrategy CreateEmailStrategy(EmailRequest emailContract)
  {
    return emailContract.EmailType switch
    {
      EmailType.Welcome => new WelcomeStrategy(emailContract, _emailLinksConfiguration, _htmlRenderer),
      EmailType.WelcomePasswordSet => new WelcomePasswordSetStrategy(emailContract, _emailLinksConfiguration, _htmlRenderer),
      EmailType.PasswordReset => new PasswordResetStrategy(emailContract, _emailLinksConfiguration, _htmlRenderer),
      EmailType.PasswordUpdate => new PasswordUpdateStrategy(emailContract, _htmlRenderer),
      EmailType.RobotStuck => new RobotStuckStrategy(emailContract, _htmlRenderer),
      EmailType.TriggerFailed => new TriggerFailedStrategy(emailContract, _htmlRenderer),
      EmailType.AutoTaskAbort => new AutoTaskAbortStrategy(emailContract, _htmlRenderer),
      _ => throw new ArgumentOutOfRangeException(nameof(emailContract.EmailType)),
    };
  }

  public async Task SendEmailAsync(EmailRequest emailContract)
  {
    using var client = new SmtpClient();
    await client.ConnectAsync(_emailConfiguration.Host, _emailConfiguration.Port, _emailConfiguration.SecureSocketOptions);
    await client.AuthenticateAsync(_emailConfiguration.Username, _emailConfiguration.Password);
    var emailStrategy = CreateEmailStrategy(emailContract);
    var messages = await emailStrategy.BuildEmailAsync();
    foreach (var message in messages)
    {
      message.From.Add(new MailboxAddress(_emailConfiguration.FromName, _emailConfiguration.FromAddress));
      await client.SendAsync(message);
    }
    await client.DisconnectAsync(true);
    await _activityLogService.CreateActivityLogAsync(new ActivityLogRequest
    {
      EntityName = "Email",
      EntityId = ((int)emailContract.EmailType).ToString(),
      Action = ActivityAction.SendEmail,
      Note = $"Number of receipients: {messages.Count()}"
    });
  }
}