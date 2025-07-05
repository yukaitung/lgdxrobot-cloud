using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Data.Models.Business.Administration;

public class ActivityLogListBusinessModel
{
  public int Id { get; set; }

  public required string EntityName { get; set; }

  public required string EntityId { get; set; }

  public required ActivityAction Action { get; set; }

  public Guid? UserId { get; set; }

  public string? UserName { get; set; }

  public DateTime CreatedAt { get; set; }
}

public static class ActivityLogListBusinessModelExtensions
{
  public static ActivityLogListDto ToDto(this ActivityLogListBusinessModel activityLogBusinessModel)
  {
    return new ActivityLogListDto
    {
      Id = activityLogBusinessModel.Id,
      EntityName = activityLogBusinessModel.EntityName,
      EntityId = activityLogBusinessModel.EntityId,
      Action = activityLogBusinessModel.Action,
      User = activityLogBusinessModel.UserId != null ? new LgdxUserSearchDto
      {
        Id = activityLogBusinessModel.UserId.Value,
        UserName = activityLogBusinessModel.UserName
      } : null,
      CreatedAt = activityLogBusinessModel.CreatedAt,
    };
  }

  public static IEnumerable<ActivityLogListDto> ToDto(this IEnumerable<ActivityLogListBusinessModel> activityLogBusinessModels)
  {
    return activityLogBusinessModels.Select(t => t.ToDto());
  }
}