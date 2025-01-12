using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Automation;

public record TaskDetailBody : IValidatableObject
{
  public int? Id { get; set; }

  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  
  public int? WaypointId { get; set; }

  public string? WaypointName { get; set; }

  public int Order { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (WaypointId == null && CustomX == null && CustomY == null && CustomRotation == null)
    {
      yield return new ValidationResult("Please enter a waypoint or a custom coordinate.", [nameof(AutoTaskDetailViewModel.AutoTaskDetails)]);
    }
  }
}

public class AutoTaskDetailViewModel : FormViewModel, IValidatableObject
{
  public int Id { get; set; }

  public string? Name { get; set; }

  public List<TaskDetailBody> AutoTaskDetails { get; set; } = [];

  [Required (ErrorMessage = "Please enter a priority.")]
  public int Priority { get; set; } = 0;

  [Required (ErrorMessage = "Please select a Flow.")]
  public int? FlowId { get; set; } = null;

  public string? FlowName { get; set; } = null;

  [Required (ErrorMessage = "Please select a Realm.")]
  public int? RealmId { get; set; } = null;

  public string? RealmName { get; set; } = null;

  public Guid? AssignedRobotId { get; set; } = null;

  public string? AssignedRobotName { get; set; } = null;

  public int CurrentProgressId { get; set; }

  public string CurrentProgressName { get; set; } = string.Empty;

  public bool IsTemplate { get; set; } = false;

  public bool IsClone { get; set; } = false;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    foreach (var autoTaskDetail in AutoTaskDetails)
    {
      List<ValidationResult> validationResults = [];
      var vc = new ValidationContext(autoTaskDetail);
      Validator.TryValidateObject(autoTaskDetail, vc, validationResults, true);
      foreach (var validationResult in validationResults)
      {
        yield return validationResult;
      }
    }
  }
}

public static class AutoTaskDetailViewModelExtensions
{
  public static void FromDto(this AutoTaskDetailViewModel autoTaskDetailViewModel, AutoTaskDto autoTaskDto)
  {
    autoTaskDetailViewModel.Id = (int)autoTaskDto.Id!;
    autoTaskDetailViewModel.Name = autoTaskDto.Name!;
    autoTaskDetailViewModel.Priority = (int)autoTaskDto.Priority!;
    autoTaskDetailViewModel.FlowId = autoTaskDto.Flow!.Id;
    autoTaskDetailViewModel.FlowName = autoTaskDto.Flow!.Name;
    autoTaskDetailViewModel.RealmId = autoTaskDto.Realm!.Id;
    autoTaskDetailViewModel.RealmName = autoTaskDto.Realm!.Name;
    autoTaskDetailViewModel.AssignedRobotId = autoTaskDto.AssignedRobot?.Id;
    autoTaskDetailViewModel.AssignedRobotName = autoTaskDto.AssignedRobot?.Name;
    autoTaskDetailViewModel.CurrentProgressId = (int)autoTaskDto.CurrentProgress!.Id!;
    autoTaskDetailViewModel.CurrentProgressName = autoTaskDto.CurrentProgress!.Name!;
    autoTaskDetailViewModel.AutoTaskDetails = autoTaskDto.AutoTaskDetails!.Select(t => new TaskDetailBody {
      Id = t.Id,
      CustomX = t.CustomX,
      CustomY = t.CustomY,
      CustomRotation = t.CustomRotation,
      WaypointId = t.Waypoint?.Id,
      WaypointName = t.Waypoint?.Name,
      Order = (int)t.Order!
    }).ToList();
  }

  public static AutoTaskUpdateDto ToUpdateDto(this AutoTaskDetailViewModel autoTaskDetailViewModel)
  {
    return new AutoTaskUpdateDto {
      Name = autoTaskDetailViewModel.Name,
      Priority = autoTaskDetailViewModel.Priority,
      FlowId = autoTaskDetailViewModel.FlowId,
      RealmId = autoTaskDetailViewModel.RealmId,
      AssignedRobotId = autoTaskDetailViewModel.AssignedRobotId,
      AutoTaskDetails = autoTaskDetailViewModel.AutoTaskDetails.Select(t => new AutoTaskDetailUpdateDto{
        Id = t.Id,
        CustomX = t.CustomX,
        CustomY = t.CustomY,
        CustomRotation = t.CustomRotation,
        WaypointId = t.WaypointId,
        Order = t.Order
      }).ToList()
    };
  }

  public static AutoTaskCreateDto ToCreateDto(this AutoTaskDetailViewModel autoTaskDetailViewModel)
  {
    return new AutoTaskCreateDto {
      Name = autoTaskDetailViewModel.Name,
      Priority = autoTaskDetailViewModel.Priority,
      FlowId = autoTaskDetailViewModel.FlowId,
      RealmId = autoTaskDetailViewModel.RealmId,
      AssignedRobotId = autoTaskDetailViewModel.AssignedRobotId,
      AutoTaskDetails = autoTaskDetailViewModel.AutoTaskDetails.Select(t => new AutoTaskDetailCreateDto{
        CustomX = t.CustomX,
        CustomY = t.CustomY,
        CustomRotation = t.CustomRotation,
        WaypointId = t.WaypointId,
        Order = t.Order
      }).ToList()
    };
  }
}