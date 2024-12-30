using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class TriggerDto
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string Url { get; set; } = null!;
  public int HttpMethodId { get; set; }
  public string? Body { get; set; }
  public bool SkipOnFailure { get; set; }
  public int? ApiKeyInsertLocationId { get; set; }
  public string? ApiKeyFieldName { get; set; }
  public ApiKeyDto? ApiKey { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
