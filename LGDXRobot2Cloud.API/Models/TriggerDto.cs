namespace LGDXRobot2Cloud.API.Models
{
  public class TriggerDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string ApiKeyLocation { get; set; } = string.Empty;
    public string ApiKeyName { get; set; } = string.Empty;
    public ApiKeyDto ApiKey { get; set; } = new ApiKeyDto();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}