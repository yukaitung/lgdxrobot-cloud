namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record RobotListDto
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public required RealmSearchDto Realm { get; set; }
}
