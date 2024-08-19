using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.Blazor;

public class NodesCollectionBlazor
{
  public int Id { get; set; }

  [Required]
  public string Name { get; set; } = null!;
  
  [Required]
  public IList<NodesCollectionDetailBlazor> Nodes { get; set; } = [];
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
