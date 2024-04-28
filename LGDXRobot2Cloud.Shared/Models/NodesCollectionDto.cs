namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodesCollectionDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public IEnumerable<NodesCollectionDetailDto> Nodes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}