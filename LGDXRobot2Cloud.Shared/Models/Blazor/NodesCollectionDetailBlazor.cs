using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class NodesCollectionDetailBlazor
  {
    public int? Id { get; set; }
    public NodeBlazor? Node { get; set; } = null!;

    [Required]
    public int? NodeId { get; set; }
    public string? NodeName { get; set; }
    public bool AutoRestart { get; set; }
  }
}