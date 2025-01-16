using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Automation;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record FlowCreateDto : IValidatableObject
{
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  public required IList<FlowDetailCreateDto> FlowDetails { get; set; } = [];

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

public static class FlowCreateDtoExtensions
{
  public static FlowCreateBusinessModel ToBusinessModel(this FlowCreateDto flowCreateDto)
  {
    return new FlowCreateBusinessModel {
      Name = flowCreateDto.Name,
      FlowDetails = flowCreateDto.FlowDetails.Select(fd => fd.ToBusinessModel()).ToList(),
    };
  }
}