using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.RabbitMQ;

namespace LGDXRobotCloud.Worker.Services;

public interface IActivityLogService
{
  Task CreateActivityLogAsync(ActivityLogContract activityLogContract);
}

partial class ActivityLogService(
    LgdxLogsContext LgdxLogsContext
  ) : IActivityLogService
{
  private readonly LgdxLogsContext _lgdxLogsContext = LgdxLogsContext ?? throw new ArgumentNullException(nameof(LgdxLogsContext));

  public async Task CreateActivityLogAsync(ActivityLogContract activityLogContract)
  {
    var activityLog = new ActivityLog
    {
      EntityName = activityLogContract.EntityName,
      EntityId = activityLogContract.EntityId,
      Action = (int)activityLogContract.Action,
      UserId = activityLogContract.UserId,
      ApiKeyId = activityLogContract.ApiKeyId,
      Note = activityLogContract.Note,
      CreatedAt = DateTime.UtcNow,
    };

    await _lgdxLogsContext.ActivityLogs.AddAsync(activityLog);
    await _lgdxLogsContext.SaveChangesAsync();
  }
}