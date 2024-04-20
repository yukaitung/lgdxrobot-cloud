namespace LGDXRobot2Cloud.Shared.Models
{
  public class FlowDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public IEnumerable<FlowDetailDto> FlowDetails { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}