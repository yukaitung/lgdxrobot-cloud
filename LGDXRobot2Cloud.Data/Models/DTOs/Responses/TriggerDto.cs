namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class TriggerDto
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string Url { get; set; } = null!;
  public string? Body { get; set; }
  public int? ApiKeyInsertLocationId { get; set; }
  public string? ApiKeyFieldName { get; set; }
  public ApiKeyDto? ApiKey { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
