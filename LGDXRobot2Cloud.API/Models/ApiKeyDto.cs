namespace LGDXRobot2Cloud.API.Models
{
  public class ApiKeyDto
  {
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Key { get; set; }
    public bool isThirdParty { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}