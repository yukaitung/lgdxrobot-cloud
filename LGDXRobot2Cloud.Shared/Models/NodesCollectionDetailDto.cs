namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodesCollectionDetailDto
  {
    public int Id { get; set; }
    public required NodeDto Node { get; set; }
    public bool AutoRestart { get; set; }
  }
}