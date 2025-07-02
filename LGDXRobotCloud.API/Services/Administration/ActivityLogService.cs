using System.Security.Claims;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.Models.Business.Administration;
using MassTransit;

namespace LGDXRobotCloud.API.Services.Administration;

public interface IActivityLogService
{
  Task AddActivityLogAsync(ActivityLogCreateBusinessModel activityLogCreateBusinessMode);
}

public class ActivityLogService(
    IBus bus,
    IHttpContextAccessor httpContextAccessor
  ) : IActivityLogService
{
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

  public async Task AddActivityLogAsync(ActivityLogCreateBusinessModel activityLogCreateBusinessModel)
  {
    var httpContext = _httpContextAccessor.HttpContext;
    var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var apiKeyId = httpContext?.Items["ApiKeyId"] as int?;
    await _bus.Publish(new ActivityLogContract
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