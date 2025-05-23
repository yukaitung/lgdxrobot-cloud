using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record AutoTaskListBusinessModel
{
  public required int Id { get; set; }

  public string? Name { get; set; }

  public required int Priority { get; set; }

  public int? FlowId { get; set; }

  public string? FlowName { get; set; }

  public required int RealmId { get; set; }

  public required string RealmName { get; set; }

  public Guid? AssignedRobotId { get; set; }

  public string? AssignedRobotName { get; set; }

  public required int CurrentProgressId { get; set; }

  public required string CurrentProgressName { get; set; }
}

public static class AutoTaskListBusinessModelExtensions
{
  public static AutoTaskListDto ToDto(this AutoTaskListBusinessModel model)
  {
    return new AutoTaskListDto {
      Id = model.Id,
      Name = model.Name,
      Priority = model.Priority,
      Flow = new FlowSearchDto {
        Id = model.FlowId ?? 0,
        Name = model.FlowName ?? "Deleted Flow",
      },
      Realm = new RealmSearchDto {
        Id = model.RealmId,
        Name = model.RealmName,
      },
      AssignedRobot = model.AssignedRobotId == null ? null : new RobotSearchDto {
        Id = model.AssignedRobotId!.Value,
        Name = model.AssignedRobotName!,
      },
      CurrentProgress = new ProgressSearchDto {
        Id = model.CurrentProgressId,
        Name = model.CurrentProgressName,
      },
    };
  }

  public static IEnumerable<AutoTaskListDto> ToDto(this IEnumerable<AutoTaskListBusinessModel> model)
  {
    return model.Select(m => m.ToDto());
  }
}