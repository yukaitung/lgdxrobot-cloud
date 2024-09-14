using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.Models;

public class NodesCollectionDetail
{
  public int? Id { get; set; }

  public Node? Node { get; set; } = null!;

  [Required]
  public int? NodeId { get; set; }

  public string? NodeName { get; set; }
  
  public bool AutoRestart { get; set; }
}
