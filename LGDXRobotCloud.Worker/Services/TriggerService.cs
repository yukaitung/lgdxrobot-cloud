using System.Text;
using System.Text.Json;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Emails;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Worker.Services;

public interface ITriggerService
{
  Task TriggerAsync(AutoTaskTriggerContract autoTaskTriggerContract);
}

public class TriggerService(
    LgdxContext context,
    IEmailService emailService,
    HttpClient httpClient
  ) : ITriggerService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly HttpClient _httpClient = httpClient;

  public async Task TriggerAsync(AutoTaskTriggerContract autoTaskTriggerContract)
  {
    var trigger = autoTaskTriggerContract.Trigger;
    var body = autoTaskTriggerContract.Body;
    var apiKeyFieldName = trigger.ApiKeyFieldName ?? "Key";

    if (trigger.ApiKeyId != null)
    {
      var apiKey = await _context.ApiKeys.Where(a => a.Id == trigger.ApiKeyId).FirstOrDefaultAsync();
      switch (trigger.ApiKeyInsertLocationId)
      {
        case (int) ApiKeyInsertLocation.Header:
          _httpClient.DefaultRequestHeaders.Add(apiKeyFieldName, apiKey?.Secret);
          break;
        case (int) ApiKeyInsertLocation.Body:
          body.Add(apiKeyFieldName, apiKey?.Secret ?? string.Empty);
          break;
      }
    }

    var bodyStr = JsonSerializer.Serialize(body);
    var requestBody = new StringContent(bodyStr, Encoding.UTF8, "application/json");
    HttpResponseMessage? httpResult = null;
    switch (trigger.HttpMethodId)
    {
      case (int) TriggerHttpMethod.Get:
        httpResult = await _httpClient.GetAsync(trigger.Url);
        break;
      case (int) TriggerHttpMethod.Post:
        httpResult = await _httpClient.PostAsync(trigger.Url, requestBody);
        break;
      case (int) TriggerHttpMethod.Put:
        httpResult = await _httpClient.PutAsync(trigger.Url, requestBody);
        break;
      case (int) TriggerHttpMethod.Patch:
        httpResult = await _httpClient.PatchAsync(trigger.Url, requestBody);
        break;
      case (int) TriggerHttpMethod.Delete:
        httpResult = await _httpClient.DeleteAsync(trigger.Url);
        break;
      default:
        break;
    }
    if (httpResult != null && !httpResult.IsSuccessStatusCode) 
    {
      // Add to trigger retry
      body.Remove(apiKeyFieldName);
      await _context.TriggerRetries.AddAsync(new TriggerRetry{
        TriggerId = trigger.Id,
        AutoTaskId = autoTaskTriggerContract.AutoTaskId,
        Body = JsonSerializer.Serialize(body)
      });
      await _context.SaveChangesAsync();

      // Send email
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
      if (recipients != null && recipients.Count >= 0)
      {
        await _emailService.SendEmailAsync(new EmailContract {
          EmailType = EmailType.TriggerFailed,
          Recipients = recipients,
          Metadata = JsonSerializer.Serialize(new TriggerFailedViewModel {
            TriggerId = trigger.Id.ToString(),
            TriggerName = trigger.Name,
            TriggerUrl = trigger.Url,
            HttpMethodId = trigger.HttpMethodId.ToString(),
            StatusCode = ((int)httpResult.StatusCode).ToString(),
            Reason = httpResult.ReasonPhrase ?? string.Empty,
            AutoTaskId = autoTaskTriggerContract.AutoTaskId.ToString(),
            AutoTaskName = autoTaskTriggerContract.AutoTaskName,
            RobotId = autoTaskTriggerContract.RobotId.ToString(),
            RobotName = autoTaskTriggerContract.RobotName,
            RealmId = autoTaskTriggerContract.RealmId.ToString(),
            RealmName = autoTaskTriggerContract.RealmName,
          })
        });
      }
    }
  }
}