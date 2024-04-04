using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class NodesCollectionDetailCreateDto
  {
    [Required]
    public int NodeId { get; set; }
    public bool AutoRestart { get; set; }
  }
}