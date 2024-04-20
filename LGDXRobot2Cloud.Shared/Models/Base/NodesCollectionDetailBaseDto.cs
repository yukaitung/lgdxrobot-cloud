using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class NodesCollectionDetailBaseDto
  {
    [Required]
    public int NodeId { get; set; }
    public bool AutoRestart { get; set; }
  }
}