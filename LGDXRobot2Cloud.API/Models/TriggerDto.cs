namespace LGDXRobot2Cloud.API.Models
{
  public class TriggerDto
  {
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public string? Body { get; set; }
    public required string ApiKeyLocation { get; set; }
    public required string ApiKeyName { get; set; }
    public ApiKeyDto ApiKey { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}