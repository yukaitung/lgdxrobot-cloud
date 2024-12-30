using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Automation;

public record TaskDetailBody
{
  public int? Id { get; set; }

  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  
  public int? WaypointId { get; set; }

  public string? WaypointName { get; set; }

  [Required]
  public int Order { get; set; }
}

public class AutoTaskDetailViewModel : FormViewModel
{
  public int Id { get; set; }

  public string? Name { get; set; }

  public List<TaskDetailBody> AutoTaskDetails { get; set; } = [];

  public int Priority { get; set; } = 0;

  [Required]
  public int? FlowId { get; set; } = null;

  public string? FlowName { get; set; } = null;

  public int? RealmId { get; set; } = null;

  public string? RealmName { get; set; } = null;

  public Guid? AssignedRobotId { get; set; } = null;

  public string? AssignedRobotName { get; set; } = null;

  public int CurrentProgressId { get; set; }

  public string CurrentProgressName { get; set; } = string.Empty;

  public bool IsTemplate { get; set; } = false;

  public bool IsClone { get; set; } = false;
}