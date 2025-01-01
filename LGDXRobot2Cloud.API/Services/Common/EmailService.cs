using System.Text.Json;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Models.Emails;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.Utilities.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Services.Common;

public interface IEmailService
{
  Task SendWelcomeEmailAsync(string recipientEmail, string recipientName, WelcomeViewModel welcomeViewModel);
  Task SendWellcomePasswordSetEmailAsync(string recipientEmail, string recipientName, WelcomePasswordSetViewModel welcomePasswordSetViewModel);
  Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, PasswordResetViewModel passwordResetViewModel);
  Task SendPasswordUpdateEmailAsync(string recipientEmail, string recipientName, PasswordUpdateViewModel passwordUpdateViewModel);
  Task SendRobotStuckEmailAsync(Guid robotId, double x, double y);
}

public sealed class EmailService(
    IBus bus,
    LgdxContext context
  ) : IEmailService
  {
    private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

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

    public async Task SendRobotStuckEmailAsync(Guid robotId, double x, double y)
    {
      List<string> recipientIds = await _context.UserRoles.AsNoTracking()
        .Where(ur => ur.RoleId == LgdxRolesHelper.GetSystemRoleId(LgdxRoleType.EmailRecipient).ToString())
        .Select(ur => ur.UserId).ToListAsync();
      var recipients = await _context.Users.AsNoTracking()
        .Where(u => recipientIds.Contains(u.Id))
        .Select(u => new EmailRecipient
        {
          Email = u.Email!,
          Name = u.Name!
        })
        .ToListAsync();
      if (recipients == null || recipients.Count == 0)
      {
        return;
      }
      var viewModel = await _context.Robots.AsNoTracking()
        .Where(r => r.Id == robotId)
        .Include(r => r.Realm)
        .Select(r => new RobotStuckViewModel {
            RobotId = r.Id.ToString(),
            RobotName = r.Name,
            RealmId = r.Realm.Id.ToString(),
            RealmName = r.Realm.Name,
            Time = DateTime.Now.ToString("dd MMMM yyyy, hh:mm:ss tt"),
            X = @Math.Round(x, 4).ToString(),
            Y = @Math.Round(y, 4).ToString()
          })
        .FirstOrDefaultAsync();
      if (viewModel != null)
      {
        var emailContract = new EmailContract
        {
          EmailType = EmailType.RobotStuck,
          Recipients = recipients,
          Metadata = JsonSerializer.Serialize(viewModel)
        };
        await _bus.Publish(emailContract);
      }
    }
  }