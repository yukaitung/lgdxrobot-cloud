namespace LGDXRobot2Cloud.Shared.Models
{
  public class TriggerListDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}