namespace LGDXRobot2Cloud.Shared.Models
{
  public class ApiKeyDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsThirdParty { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}