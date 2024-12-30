using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Automation;

public record FlowDetailBody
{
  public int? Id { get; set; } = null;
  
  public int Order { get; set; }

  [Required]
  public int? ProgressId { get; set; } = null;

  public string? ProgressName { get; set; } = null;

  public int AutoTaskNextControllerId { get; set; } = 1;

  public int? TriggerId { get; set; } = null;

  public string? TriggerName { get; set; } = null;
}

public class FlowDetailViewModel : FormViewModel
{
  public int Id { get; set; }

  [Required]
  public string Name { get; set; } = null!;

  public List<FlowDetailBody> FlowDetails { get; set; } = [];
}