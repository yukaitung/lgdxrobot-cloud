using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Automation;

public record FlowDetailBody
{
  public int? Id { get; set; } = null;
  
  public int Order { get; set; }

  [Required (ErrorMessage = "Please select a progress.")]
  public int? ProgressId { get; set; } = null;

  public string? ProgressName { get; set; } = null;

  public int AutoTaskNextControllerId { get; set; } = 1;

  public int? TriggerId { get; set; } = null;

  public string? TriggerName { get; set; } = null;
}

public class FlowDetailViewModel : FormViewModel, IValidatableObject
{
  public int Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  public List<FlowDetailBody> FlowDetails { get; set; } = [];

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    foreach (var flow in FlowDetails)
    {
      List<ValidationResult> validationResults = [];
      var vc = new ValidationContext(flow);
      Validator.TryValidateObject(flow, vc, validationResults, true);
      foreach (var validationResult in validationResults)
      {
        yield return validationResult;
      }
    }
  }
}

public static class FlowDetailViewModelExtensions
{
  public static void FromDto(this FlowDetailViewModel flowDetailViewModel, FlowDto flowDto)
  {
    flowDetailViewModel.Id = (int)flowDto.Id!;
    flowDetailViewModel.Name = flowDto.Name!;
    flowDetailViewModel.FlowDetails = flowDto.FlowDetails!.Select(t => new FlowDetailBody {
      Id = t.Id,
      Order = (int)t.Order!,
      ProgressId = t.Progress!.Id,
      ProgressName = t.Progress!.Name,
      TriggerId = t.Trigger?.Id,
      TriggerName = t.Trigger?.Name,
      AutoTaskNextControllerId = (int)t.AutoTaskNextControllerId!
    }).ToList();
  }

  public static FlowUpdateDto ToUpdateDto(this FlowDetailViewModel flowDetailViewModel)
  {
    return new FlowUpdateDto {
      Name = flowDetailViewModel.Name,
      FlowDetails = flowDetailViewModel.FlowDetails.Select(t => new FlowDetailUpdateDto {
        Id = t.Id,
        Order = t.Order!,
        ProgressId = t.ProgressId,
        TriggerId = t.TriggerId,
        AutoTaskNextControllerId = t.AutoTaskNextControllerId!
      }).ToList()
    };
  }

  public static FlowCreateDto ToCreateDto(this FlowDetailViewModel flowDetailViewModel)
  {
    return new FlowCreateDto {
      Name = flowDetailViewModel.Name,
      FlowDetails = flowDetailViewModel.FlowDetails.Select(t => new FlowDetailCreateDto {
        Order = t.Order,
        ProgressId = t.ProgressId,
        TriggerId = t.TriggerId,
        AutoTaskNextControllerId = t.AutoTaskNextControllerId!
      }).ToList()
    };
  }
}