namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodesCollectionDetailDto
  {
    public int Id { get; set; }
    public NodeDto Node { get; set; } = null!;
    public bool AutoRestart { get; set; }
  }
}