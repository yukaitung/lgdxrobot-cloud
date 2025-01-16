using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Automation;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record FlowUpdateDto
{
  [Required]
  public required string Name { get; set; }
  
  public required IList<FlowDetailUpdateDto> FlowDetails { get; set; } = [];

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

public static class FlowUpdateDtoExtensions
{
  public static FlowUpdateBusinessModel ToBusinessModel(this FlowUpdateDto flowUpdateDto)
  {
    return new FlowUpdateBusinessModel {
      Name = flowUpdateDto.Name,
      FlowDetails = flowUpdateDto.FlowDetails.Select(fd => fd.ToBusinessModel()).ToList(),
    };
  }
}
