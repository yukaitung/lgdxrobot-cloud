namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record RealmSearchDto
{
  public required int Id { get; set; }

  public required string Name { get; set; }
}