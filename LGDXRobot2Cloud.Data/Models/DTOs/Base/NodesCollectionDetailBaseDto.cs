using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Base;

public class NodesCollectionDetailBaseDto
{
  [Required]
  public int NodeId { get; set; }
  public bool AutoRestart { get; set; }
}
