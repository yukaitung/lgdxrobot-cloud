using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.Worker.Strategies.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LGDXRobot2Cloud.Worker.Services;

public interface IEmailService
{
  Task SendEmailAsync(EmailContract emailContract);
}

public sealed class EmailService (
    IOptionsSnapshot<EmailConfiguration> emailConfiguration,
    HtmlRenderer htmlRenderer
  ) : IEmailService
{
  private readonly EmailConfiguration _emailConfiguration = emailConfiguration.Value ?? throw new ArgumentNullException(nameof(emailConfiguration));
  private readonly HtmlRenderer _htmlRenderer = htmlRenderer ?? throw new ArgumentNullException(nameof(htmlRenderer));

  private IEmailStrategy CreateEmailStrategy(EmailContract emailContract)
  {
    return emailContract.EmailType switch
    {
      EmailType.Welcome => new WelcomeStrategy(emailContract, _htmlRenderer),
      _ => throw new ArgumentOutOfRangeException(nameof(emailContract.EmailType)),
    };
  }

  public async Task SendEmailAsync(EmailContract emailContract)
  {

    var emailStrategy = CreateEmailStrategy(emailContract);
    var message = await emailStrategy.BuildEmailAsync();
    message.From.Add(new MailboxAddress(_emailConfiguration.FromName, _emailConfiguration.FromAddress));
    using var client = new SmtpClient();
    await client.ConnectAsync(_emailConfiguration.Host, _emailConfiguration.Port, SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(_emailConfiguration.Username, _emailConfiguration.Password);
    await client.SendAsync(message);
    await client.DisconnectAsync(true);
  }
}