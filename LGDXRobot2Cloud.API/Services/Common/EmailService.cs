using System.Text;
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
  Task SendWelcomeEmailAsync(string recipientEmail, string recipientName, string userName);
  Task SendWellcomePasswordSetEmailAsync(string recipientEmail, string recipientName, string userName, string token);
  Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string userName, string token);
  Task SendPasswordUpdateEmailAsync(string recipientEmail, string recipientName, string userName);
  Task SendRobotStuckEmailAsync(Guid robotId, double x, double y);
  Task SendAutoTaskAbortEmailAsync(Guid robotId, int taskId, AutoTaskAbortReason autoTaskAbortReason);
}

public sealed class EmailService(
    IBus bus,
    LgdxContext context
  ) : IEmailService
{
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  private async Task<List<EmailRecipient>> GetRecipientsAsync()
  {
    List<string> recipientIds = await _context.UserRoles.AsNoTracking()
      .Where(ur => ur.RoleId == LgdxRolesHelper.GetSystemRoleId(LgdxRoleType.EmailRecipient).ToString())
      .Select(ur => ur.UserId).ToListAsync();
    return await _context.Users.AsNoTracking()
      .Where(u => recipientIds.Contains(u.Id))
      .Select(u => new EmailRecipient
      {
        Email = u.Email!,
        Name = u.Name!
      })
      .ToListAsync() ?? [];
  }

  public async Task SendWelcomeEmailAsync(string recipientEmail, string recipientName, string userName)
  {
    var emailContract = new EmailContract
    {
      EmailType = EmailType.Welcome,
      Recipients = [new EmailRecipient
      {
        Email = recipientEmail,
        Name = recipientName
      }],
      Metadata = JsonSerializer.Serialize(new WelcomeViewModel {
        UserName = userName
      })
    };
    await _bus.Publish(emailContract);
  }

  public async Task SendWellcomePasswordSetEmailAsync(string recipientEmail, string recipientName, string userName, string token)
  {
    var emailContract = new EmailContract
    {
      EmailType = EmailType.WelcomePasswordSet,
      Recipients = [new EmailRecipient
      {
        Email = recipientEmail,
        Name = recipientName
      }],
      Metadata = JsonSerializer.Serialize(new WelcomePasswordSetViewModel {
        UserName = userName,
        Email = recipientEmail,
        Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
      })
    };
    await _bus.Publish(emailContract);
  }

  public async Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string userName, string token)
  {
    var emailContract = new EmailContract
    {
      EmailType = EmailType.PasswordReset,
      Recipients = [new EmailRecipient
      {
        Email = recipientEmail,
        Name = recipientName
      }],
      Metadata = JsonSerializer.Serialize(new PasswordResetViewModel{
        UserName = userName,
        Email = recipientEmail,
        Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
      })
    };
    await _bus.Publish(emailContract);
  }

  public async Task SendPasswordUpdateEmailAsync(string recipientEmail, string recipientName, string userName)
  {
    var emailContract = new EmailContract
    {
      EmailType = EmailType.PasswordUpdate,
      Recipients = [new EmailRecipient
      {
        Email = recipientEmail,
        Name = recipientName
      }],
      Metadata = JsonSerializer.Serialize(new PasswordUpdateViewModel{
        UserName = userName,
        Time = DateTime.Now.ToString("dd MMMM yyyy, hh:mm:ss tt")
      })
    };
    await _bus.Publish(emailContract);
  }

  public async Task SendRobotStuckEmailAsync(Guid robotId, double x, double y)
  {
    var recipients = await GetRecipientsAsync();
    if (recipients.Count == 0)
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

  public async Task SendAutoTaskAbortEmailAsync(Guid robotId, int taskId, AutoTaskAbortReason autoTaskAbortReason)
  {
    var recipients = await GetRecipientsAsync();
    if (recipients.Count == 0)
    {
      return;
    }
    var viewModel = await _context.AutoTasks.AsNoTracking()
      .Where(at => at.Id == taskId)
      .Include(at => at.AssignedRobot)
      .ThenInclude(r => r!.Realm)
      .Select(at => new AutoTaskAbortViewModel {
          AutoTaskId = at.Id.ToString(),
          AutoTaskName = at.Name ?? string.Empty,
          AbortReason = ((int)autoTaskAbortReason).ToString(),
          RobotId = at.AssignedRobot!.Id.ToString(),
          RobotName = at.AssignedRobot.Name,
          RealmId = at.AssignedRobot.Realm.Id.ToString(),
          RealmName = at.AssignedRobot.Realm.Name,
          Time = DateTime.Now.ToString("dd MMMM yyyy, hh:mm:ss tt")
        })
      .FirstOrDefaultAsync();
      if (viewModel != null)
      {
        var emailContract = new EmailContract
        {
          EmailType = EmailType.AutoTaskAbort,
          Recipients = recipients,
          Metadata = JsonSerializer.Serialize(viewModel)
        };
        await _bus.Publish(emailContract);
      }
  }
}