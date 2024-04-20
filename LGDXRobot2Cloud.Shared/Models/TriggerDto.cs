namespace LGDXRobot2Cloud.Shared.Models
{
  public class TriggerDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string? Body { get; set; }
    public string? ApiKeyLocation { get; set; }
    public string? ApiKeyName { get; set; }
    public ApiKeyDto? ApiKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}