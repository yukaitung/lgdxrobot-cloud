using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record AutoTaskBusinessModel
{
  public required int Id { get; set; }

  public string? Name { get; set; }

  public required int Priority { get; set; }

  public required int FlowId { get; set; }

  public required string FlowName { get; set; }

  public required int RealmId { get; set; }

  public required string RealmName { get; set; }

  public Guid? AssignedRobotId { get; set; }

  public string? AssignedRobotName { get; set; }

  public required int CurrentProgressId { get; set; }

  public required string CurrentProgressName { get; set; }

  public required IEnumerable<AutoTaskDetailBusinessModel> AutoTaskDetails { get; set; } = [];
}

public static class AutoTaskBusinessModelExtensions
{
  public static AutoTaskDto ToDto(this AutoTaskBusinessModel model)
  {
    return new AutoTaskDto {
      Id = model.Id,
      Name = model.Name,
      Priority = model.Priority,
      Flow = new FlowSearchDto {
        Id = model.FlowId,
        Name = model.FlowName,
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
      AutoTaskDetails = model.AutoTaskDetails.Select(td => td.ToDto()),
    };
  }
}