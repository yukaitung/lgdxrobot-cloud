namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record TriggerDto
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required string Url { get; set; }

  public required int HttpMethodId { get; set; }

  public string? Body { get; set; }

  public required bool SkipOnFailure { get; set; }
  
  public int? ApiKeyInsertLocationId { get; set; }

  public string? ApiKeyFieldName { get; set; }

  public ApiKeySearchDto? ApiKey { get; set; }
}
