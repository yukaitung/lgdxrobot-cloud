using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Worker.Services;
using MassTransit;

namespace LGDXRobotCloud.Worker.Consumers;

public class ActivityLogConsumer(IActivityLogService activityLogService) : IConsumer<ActivityLogContract>
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));

  public async Task Consume(ConsumeContext<ActivityLogContract> context)
  {
    await _activityLogService.AddActivityLog(context.Message);
  }
}