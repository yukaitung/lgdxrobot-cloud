using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;

namespace LGDXRobotCloud.Worker.Services;

public interface IActivityLogService
{
  Task AddActivityLog(ActivityLogContract activityLogContract);
}

partial class ActivityLogService(
    ActivityContext activityContext
  ) : IActivityLogService
{
  private readonly ActivityContext _activityContext = activityContext ?? throw new ArgumentNullException(nameof(activityContext));

  public async Task AddActivityLog(ActivityLogContract activityLogContract)
  {
    var activityLog = new ActivityLog
    {
      EntityName = activityLogContract.EntityName,
      EntityId = activityLogContract.EntityId,
      Action = activityLogContract.Action,
      UserId = activityLogContract.UserId,
      ApiKeyId = activityLogContract.ApiKeyId,
      Note = activityLogContract.Note,
      CreatedAt = DateTime.UtcNow,
    };

    await _activityContext.ActivityLogs.AddAsync(activityLog);
    await _activityContext.SaveChangesAsync();
  }
}