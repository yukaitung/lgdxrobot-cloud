using System.Security.Claims;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.RabbitMQ;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace LGDXRobotCloud.API.Services.Administration;

public interface IActivityLogService
{
  Task<(IEnumerable<ActivityLogListBusinessModel>, PaginationHelper)> GetActivityLogsAsync(string? entityName, string? entityId, int pageNumber, int pageSize);
  Task<ActivityLogBusinessModel> GetActivityLogAsync(int id);
  Task CreateActivityLogAsync(ActivityLogCreateBusinessModel activityLogCreateBusinessMode);
}

public class ActivityLogService(
    LgdxLogsContext LgdxLogsContext,
    IMessageBus bus,
    IHttpContextAccessor httpContextAccessor,
    LgdxContext lgdxContext
  ) : IActivityLogService
{
  private readonly LgdxLogsContext _lgdxLogsContext = LgdxLogsContext ?? throw new ArgumentNullException(nameof(LgdxLogsContext));
  private readonly IMessageBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
  private readonly LgdxContext _lgdxContext = lgdxContext ?? throw new ArgumentNullException(nameof(lgdxContext));

  public async Task<(IEnumerable<ActivityLogListBusinessModel>, PaginationHelper)> GetActivityLogsAsync(string? entityName, string? entityId, int pageNumber, int pageSize)
  {
    var query = _lgdxLogsContext.ActivityLogs as IQueryable<ActivityLog>;
    if (!string.IsNullOrWhiteSpace(entityName))
    {
      entityName = entityName.Trim();
      query = query.Where(t => t.EntityName.ToLower().Contains(entityName.ToLower()));
    }
    if (!string.IsNullOrWhiteSpace(entityId))
    {
      entityId = entityId.Trim();
      query = query.Where(t => t.EntityId.ToLower().Contains(entityId.ToLower()));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var activityLogs = await query.AsNoTracking()
      .OrderBy(t => t.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(t => new ActivityLogListBusinessModel
      {
        Id = t.Id,
        EntityName = t.EntityName,
        EntityId = t.EntityId,
        Action = (Utilities.Enums.ActivityAction)t.Action,
        UserId = t.UserId,
        CreatedAt = t.CreatedAt,
      })
      .ToListAsync();

    // Get UserName
    HashSet<string> userIds = activityLogs.Where(t => t.UserId != null)
      .Select(t => t.UserId!.Value.ToString())
      .ToHashSet();

    Dictionary<string, string?> userNames = await _lgdxContext.Users
      .Where(t => userIds.Contains(t.Id))
      .Select(t => new { t.Id, t.UserName })
      .ToDictionaryAsync(t => t.Id, t => t.UserName);

    foreach (var activityLog in activityLogs)
    {
      if (activityLog.UserId != null)
      {
        activityLog.UserName = userNames[activityLog.UserId.ToString()!];
      }
    }

    return (activityLogs, PaginationHelper);
  }

  public async Task<ActivityLogBusinessModel> GetActivityLogAsync(int id)
  {
    var activityLog = await _lgdxLogsContext.ActivityLogs.AsNoTracking()
      .Where(t => t.Id == id)
      .Select(t => new ActivityLogBusinessModel
      {
        Id = t.Id,
        EntityName = t.EntityName,
        EntityId = t.EntityId,
        Action = (Utilities.Enums.ActivityAction)t.Action,
        UserId = t.UserId,
        ApiKeyId = t.ApiKeyId,
        Note = t.Note,
        CreatedAt = t.CreatedAt,
      })
      .SingleOrDefaultAsync() ?? throw new LgdxNotFound404Exception();

    // Get UserName
    if (activityLog.UserId != null)
    {
      activityLog.UserName = await _lgdxContext.Users
        .Where(t => t.Id == activityLog.UserId.ToString())
        .Select(t => t.UserName)
        .FirstOrDefaultAsync();
    }

    // Get ApiKeyName
    if (activityLog.ApiKeyId != null)
    {
      activityLog.ApiKeyName = await _lgdxContext.ApiKeys
        .Where(t => t.Id == activityLog.ApiKeyId)
        .Select(t => t.Name)
        .FirstOrDefaultAsync();
    } 

    return activityLog;
  }

  public async Task CreateActivityLogAsync(ActivityLogCreateBusinessModel activityLogCreateBusinessModel)
  {
    var httpContext = _httpContextAccessor.HttpContext;
    var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var apiKeyId = httpContext?.Items["ApiKeyId"] as int?;
    await _bus.PublishAsync(new ActivityLogRequest
    {
      EntityName = activityLogCreateBusinessModel.EntityName,
      EntityId = activityLogCreateBusinessModel.EntityId,
      Action = activityLogCreateBusinessModel.Action,
      UserId = userId != null ? Guid.Parse(userId) : null,
      ApiKeyId = apiKeyId,
      Note = activityLogCreateBusinessModel.Note,
    });
  }
}