namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record AutoTaskListDto
{
  public required int Id { get; set; }

  public string? Name { get; set; }

  public required int Priority { get; set; }

  public required FlowSearchDto Flow { get; set; }

  public required RealmSearchDto Realm { get; set; }

  public RobotSearchDto? AssignedRobot { get; set; }

  public required ProgressSearchDto CurrentProgress { get; set; }
}
