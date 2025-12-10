using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Automation;

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
      yield return new ValidationResult("Please enter a waypoint or a custom coordinate.", [nameof(AutoTaskDetailsViewModel.AutoTaskDetails)]);
    }
  }
}

public record AutoTaskJourney
{
  public int Id { get; set; }

  public int? CurrentProcessId { get; set; }

  public string? CurrentProcessName { get; set; }

  public DateTimeOffset? CreatedAt { get; set; }
}

public class AutoTaskDetailsViewModel : FormViewModel, IValidatableObject
{
  public int Id { get; set; }

  public string? Name { get; set; }

  public List<TaskDetailBody> AutoTaskDetails { get; set; } = [];

  public List<AutoTaskJourney> AutoTaskJourneys { get; set; } = [];

  [Required(ErrorMessage = "Please enter a priority.")]
  public int Priority { get; set; } = 0;

  [Required(ErrorMessage = "Please select a Flow.")]
  public int? FlowId { get; set; } = null;

  public string? FlowName { get; set; } = null;

  [Required(ErrorMessage = "Please select a Realm.")]
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

public static class AutoTaskDetailsViewModelExtensions
{
  public static void FromDto(this AutoTaskDetailsViewModel AutoTaskDetailsViewModel, AutoTaskDto autoTaskDto)
  {
    AutoTaskDetailsViewModel.Id = (int)autoTaskDto.Id!;
    AutoTaskDetailsViewModel.Name = autoTaskDto.Name!;
    AutoTaskDetailsViewModel.Priority = (int)autoTaskDto.Priority!;
    AutoTaskDetailsViewModel.FlowId = autoTaskDto.Flow!.Id;
    AutoTaskDetailsViewModel.FlowName = autoTaskDto.Flow!.Name;
    AutoTaskDetailsViewModel.RealmId = autoTaskDto.Realm!.Id;
    AutoTaskDetailsViewModel.RealmName = autoTaskDto.Realm!.Name;
    AutoTaskDetailsViewModel.AssignedRobotId = autoTaskDto.AssignedRobot?.Id;
    AutoTaskDetailsViewModel.AssignedRobotName = autoTaskDto.AssignedRobot?.Name;
    AutoTaskDetailsViewModel.CurrentProgressId = (int)autoTaskDto.CurrentProgress!.Id!;
    AutoTaskDetailsViewModel.CurrentProgressName = autoTaskDto.CurrentProgress!.Name!;
    AutoTaskDetailsViewModel.AutoTaskDetails = autoTaskDto.AutoTaskDetails!.Select(t => new TaskDetailBody
    {
      Id = t.Id,
      CustomX = t.CustomX,
      CustomY = t.CustomY,
      CustomRotation = t.CustomRotation,
      WaypointId = t.Waypoint?.Id,
      WaypointName = t.Waypoint?.Name,
      Order = (int)t.Order!
    }).ToList();
    AutoTaskDetailsViewModel.AutoTaskJourneys = autoTaskDto.AutoTaskJourneys!.Select(t => new AutoTaskJourney {
      Id = (int)t.Id!,
      CurrentProcessId = t.CurrentProcessId,
      CurrentProcessName = t.CurrentProcessName ?? "Deleted Process",
      CreatedAt = t.CreatedAt
    }).ToList();
  }

  public static AutoTaskUpdateDto ToUpdateDto(this AutoTaskDetailsViewModel AutoTaskDetailsViewModel)
  {
    return new AutoTaskUpdateDto {
      Name = AutoTaskDetailsViewModel.Name,
      Priority = AutoTaskDetailsViewModel.Priority,
      FlowId = AutoTaskDetailsViewModel.FlowId,
      AssignedRobotId = AutoTaskDetailsViewModel.AssignedRobotId,
      AutoTaskDetails = AutoTaskDetailsViewModel.AutoTaskDetails.Select(t => new AutoTaskDetailUpdateDto{
        Id = t.Id,
        CustomX = t.CustomX,
        CustomY = t.CustomY,
        CustomRotation = t.CustomRotation,
        WaypointId = t.WaypointId,
        Order = t.Order
      }).ToList()
    };
  }

  public static AutoTaskCreateDto ToCreateDto(this AutoTaskDetailsViewModel AutoTaskDetailsViewModel)
  {
    return new AutoTaskCreateDto {
      Name = AutoTaskDetailsViewModel.Name,
      Priority = AutoTaskDetailsViewModel.Priority,
      FlowId = AutoTaskDetailsViewModel.FlowId,
      RealmId = AutoTaskDetailsViewModel.RealmId,
      IsTemplate = AutoTaskDetailsViewModel.IsTemplate,
      AssignedRobotId = AutoTaskDetailsViewModel.AssignedRobotId,
      AutoTaskDetails = AutoTaskDetailsViewModel.AutoTaskDetails.Select(t => new AutoTaskDetailCreateDto{
        CustomX = t.CustomX,
        CustomY = t.CustomY,
        CustomRotation = t.CustomRotation,
        WaypointId = t.WaypointId,
        Order = t.Order
      }).ToList()
    };
  }
}