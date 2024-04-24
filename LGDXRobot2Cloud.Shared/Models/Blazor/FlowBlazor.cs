using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class FlowBlazor
  {
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = null!;

    public IList<FlowDetailBlazor> FlowDetails { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
  }
}