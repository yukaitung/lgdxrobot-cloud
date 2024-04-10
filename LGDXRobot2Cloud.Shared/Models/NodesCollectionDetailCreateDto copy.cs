using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodesCollectionDetailUpdateDto
  {
    public int? Id { get; set;}
    
    [Required]
    public int NodeId { get; set; }
    public bool AutoRestart { get; set; }
  }
}