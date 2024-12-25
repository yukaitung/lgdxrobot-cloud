using System.Text;
using LGDXRobot2Cloud.API.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LGDXRobot2Cloud.API.Services;

public interface IEmailService
{
  Task SendPasswordUpdateEmailAsync(string receipentName, string receipentAddress);
  Task SendForgotPasswordEmailAsync(string receipentName, string receipentAddress, string token);
}

public sealed class EmailService(
    IOptionsSnapshot<EmailConfiguration> emailConfiguration
  ) : IEmailService
{
  private readonly EmailConfiguration _emailConfiguration = emailConfiguration.Value ?? throw new ArgumentNullException(nameof(emailConfiguration));

  private async Task SendEmailAsync(MimeMessage message)
  {
    using var client = new SmtpClient();
    await client.ConnectAsync(_emailConfiguration.Host, _emailConfiguration.Port, SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(_emailConfiguration.Username, _emailConfiguration.Password);
    await client.SendAsync(message);
    await client.DisconnectAsync(true);
  }

  public async Task SendPasswordUpdateEmailAsync(string receipentName, string receipentAddress)
  {
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(_emailConfiguration.FromName, _emailConfiguration.FromAddress));
    message.To.Add(new MailboxAddress(receipentName, receipentAddress));
    message.Subject = "[LGDXRobot2] Password Update";
    message.Body = new TextPart("plain")
    {
      Text = "Your password has been updated."
    };
    await SendEmailAsync(message);
  }

  public async Task SendForgotPasswordEmailAsync(string receipentName, string receipentAddress, string token)
  {
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(_emailConfiguration.FromName, _emailConfiguration.FromAddress));
    message.To.Add(new MailboxAddress(receipentName, receipentAddress));
    message.Subject = "[LGDXRobot2] Password Reset";
    message.Body = new TextPart("plain")
    {
      Text = $"Your password reset link is {_emailConfiguration.PasswordResetUri}?Token={Convert.ToBase64String(Encoding.UTF8.GetBytes(token))}&Email={receipentAddress}"
    };
    await SendEmailAsync(message);
  }
}