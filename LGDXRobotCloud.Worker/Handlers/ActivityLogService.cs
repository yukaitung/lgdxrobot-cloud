using LGDXRobotCloud.Data.Models.RabbitMQ;
using LGDXRobotCloud.Worker.Services;

namespace LGDXRobotCloud.Worker.Handlers;

public class ActivityLogHandler(IActivityLogService activityLogService)
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));

  public async Task Handle(ActivityLogContract activityLogRequest)
  {
    await _activityLogService.CreateActivityLogAsync(activityLogRequest);
  }
}