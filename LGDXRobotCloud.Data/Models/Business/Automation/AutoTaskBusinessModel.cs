using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Data.Models.Redis;

namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record AutoTaskBusinessModel
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

  public required IEnumerable<AutoTaskDetailBusinessModel> AutoTaskDetails { get; set; } = [];

  public IEnumerable<AutoTaskJourneyBusinessModel> AutoTaskJourneys { get; set; } = [];
}

public static class AutoTaskBusinessModelExtensions
{
  public static AutoTaskDto ToDto(this AutoTaskBusinessModel model)
  {
    return new AutoTaskDto
    {
      Id = model.Id,
      Name = model.Name,
      Priority = model.Priority,
      Flow = new FlowSearchDto
      {
        Id = model.FlowId ?? 0,
        Name = model.FlowName ?? "Deleted Flow",
      },
      Realm = new RealmSearchDto
      {
        Id = model.RealmId,
        Name = model.RealmName,
      },
      AssignedRobot = model.AssignedRobotId == null ? null : new RobotSearchDto
      {
        Id = model.AssignedRobotId!.Value,
        Name = model.AssignedRobotName!,
      },
      CurrentProgress = new ProgressSearchDto
      {
        Id = model.CurrentProgressId,
        Name = model.CurrentProgressName,
      },
      AutoTaskDetails = model.AutoTaskDetails.Select(td => td.ToDto()),
      AutoTaskJourneys = model.AutoTaskJourneys.Select(tj => tj.ToDto()),
    };
  }

  public static AutoTaskUpdate ToContract(this AutoTaskBusinessModel model)
  {
    return new AutoTaskUpdate {
      Id = model.Id,
      Name = model.Name,
      Priority = model.Priority,
      FlowId = model.FlowId ?? 0,
      FlowName = model.FlowName ?? "Deleted Flow",
      RealmId = model.RealmId,
      AssignedRobotId = model.AssignedRobotId,
      AssignedRobotName = model.AssignedRobotName,
      CurrentProgressId = model.CurrentProgressId,
      CurrentProgressName = model.CurrentProgressName,
    };
  }
}