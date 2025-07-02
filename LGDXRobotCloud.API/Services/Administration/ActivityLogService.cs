using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.Models.Business.Administration;
using MassTransit;

namespace LGDXRobotCloud.API.Services.Administration;

public interface IActivityLogService
{
  Task AddActivityLog(ActivityLogCreateBusinessModel activityLogCreateBusinessMode);
}

public class ActivityLogService(
    IBus bus
  ) : IActivityLogService
{
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));

  public async Task AddActivityLog(ActivityLogCreateBusinessModel activityLogCreateBusinessModel)
  {
    await _bus.Publish(new ActivityLogContract
    {
      EntityName = activityLogCreateBusinessModel.EntityName,
      EntityId = activityLogCreateBusinessModel.EntityId,
      Action = activityLogCreateBusinessModel.Action,
      UserId = activityLogCreateBusinessModel.UserId,
      ApiKeyId = activityLogCreateBusinessModel.ApiKeyId,
      Note = activityLogCreateBusinessModel.Note,
    });
  }
}