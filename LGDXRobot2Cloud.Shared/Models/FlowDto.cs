namespace LGDXRobot2Cloud.Shared.Models
{
  public class FlowDto
  {
    public int Id { get; set; }
    public required string Name { get; set; }
    public IEnumerable<FlowDetailDto> FlowDetails { get; set; } = new List<FlowDetailDto>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}