using System.Text.Json;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Data.Models.Emails;
using LGDXRobot2Cloud.Utilities.Enums;
using MassTransit;

namespace LGDXRobot2Cloud.API.Services.Common;

public interface IEmailService
{
  Task SendWelcomeEmailAsync(string recipientEmail, string recipientName, WelcomeViewModel welcomeViewModel);
  Task SendWellcomePasswordSetEmailAsync(string recipientEmail, string recipientName, WelcomePasswordSetViewModel welcomePasswordSetViewModel);
  Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, PasswordResetViewModel passwordResetViewModel);
  Task SendPasswordUpdateEmailAsync(string recipientEmail, string recipientName, PasswordUpdateViewModel passwordUpdateViewModel);
  Task SendRobotStuckEmailAsync(RobotStuckViewModel robotStuckViewModel);
}

public sealed class EmailService(
    IBus bus
  ) : IEmailService
  {
    private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));

    public async Task SendWelcomeEmailAsync(string recipientEmail, string recipientName, WelcomeViewModel welcomeViewModel)
    {
      var emailContract = new EmailContract
      {
        EmailType = EmailType.Welcome,
        Recipients = [new EmailRecipient
        {
          Email = recipientEmail,
          Name = recipientName
        }],
        Metadata = JsonSerializer.Serialize(welcomeViewModel)
      };
      await _bus.Publish(emailContract);
    }

    public async Task SendWellcomePasswordSetEmailAsync(string recipientEmail, string recipientName, WelcomePasswordSetViewModel welcomePasswordSetViewModel)
    {
      var emailContract = new EmailContract
      {
        EmailType = EmailType.WelcomePasswordSet,
        Recipients = [new EmailRecipient
        {
          Email = recipientEmail,
          Name = recipientName
        }],
        Metadata = JsonSerializer.Serialize(welcomePasswordSetViewModel)
      };
      await _bus.Publish(emailContract);
    }

    public async Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, PasswordResetViewModel passwordResetViewModel)
    {
      var emailContract = new EmailContract
      {
        EmailType = EmailType.PasswordReset,
        Recipients = [new EmailRecipient
        {
          Email = recipientEmail,
          Name = recipientName
        }],
        Metadata = JsonSerializer.Serialize(passwordResetViewModel)
      };
      await _bus.Publish(emailContract);
    }

    public async Task SendPasswordUpdateEmailAsync(string recipientEmail, string recipientName, PasswordUpdateViewModel passwordUpdateViewModel)
    {
      var emailContract = new EmailContract
      {
        EmailType = EmailType.PasswordUpdate,
        Recipients = [new EmailRecipient
        {
          Email = recipientEmail,
          Name = recipientName
        }],
        Metadata = JsonSerializer.Serialize(passwordUpdateViewModel)
      };
      await _bus.Publish(emailContract);
    }

    public async Task SendRobotStuckEmailAsync(RobotStuckViewModel robotStuckViewModel)
    {

    }
  }