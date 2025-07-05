using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Data.Models.Business.Administration;

public class ActivityLogBusinessModel
{
  public int Id { get; set; }

  public required string EntityName { get; set; }

  public required string EntityId { get; set; }

  public required ActivityAction Action { get; set; }

  public Guid? UserId { get; set; }

  public string? UserName { get; set; }

  public int? ApiKeyId { get; set; }

  public string? ApiKeyName { get; set; }

  public string? Note { get; set; }

  public DateTime CreatedAt { get; set; }
}

public static class ActivityLogBusinessModelExtensions
{
  public static ActivityLogDto ToDto(this ActivityLogBusinessModel activityLogBusinessModel)
  {
    return new ActivityLogDto
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
      ApiKey = activityLogBusinessModel.ApiKeyId != null ? new ApiKeySearchDto
      {
        Id = activityLogBusinessModel.ApiKeyId.Value,
        Name = activityLogBusinessModel.ApiKeyName!
      } : null,
      Note = activityLogBusinessModel.Note,
      CreatedAt = activityLogBusinessModel.CreatedAt,
    };
  }

  public static IEnumerable<ActivityLogDto> ToDto(this IEnumerable<ActivityLogBusinessModel> activityLogBusinessModels)
  {
    return activityLogBusinessModels.Select(t => t.ToDto());
  }
}