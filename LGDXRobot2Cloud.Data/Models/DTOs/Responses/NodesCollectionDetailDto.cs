namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class NodesCollectionDetailDto
{
  public int Id { get; set; }
  public NodeDto Node { get; set; } = null!;
  public bool AutoRestart { get; set; }
}
