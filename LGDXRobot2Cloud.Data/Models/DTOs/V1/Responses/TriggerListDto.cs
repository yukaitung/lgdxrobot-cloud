namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record TriggerListDto
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required string Url { get; set; }

  public required int HttpMethodId { get; set; }
}
