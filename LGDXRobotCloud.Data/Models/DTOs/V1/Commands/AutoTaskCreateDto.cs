using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Automation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record AutoTaskCreateDto : IValidatableObject
{
  public string? Name { get; set; }

  public required IEnumerable<AutoTaskDetailCreateDto> AutoTaskDetails { get; set; } = [];
  
  [Required (ErrorMessage = "Please enter a priority.")]
  public required int Priority { get; set; }
  
  [Required (ErrorMessage = "Please select a Flow.")]
  public required int FlowId { get; set; }

  [Required (ErrorMessage = "Please select a Realm.")]
  public required int RealmId { get; set; }

  public Guid? AssignedRobotId { get; set; }
  
  public bool IsTemplate { get; set; } = false;

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

public static class AutoTaskCreateDtoExtensions
{
  public static AutoTaskCreateBusinessModel ToBusinessModel(this AutoTaskCreateDto model)
  {
    return new AutoTaskCreateBusinessModel {
      Name = model.Name,
      AutoTaskDetails = model.AutoTaskDetails.Select(td => td.ToBusinessModel()),
      Priority = model.Priority,
      FlowId = model.FlowId,
      RealmId = model.RealmId,
      AssignedRobotId = model.AssignedRobotId,
      IsTemplate = model.IsTemplate,
    };
  }
}