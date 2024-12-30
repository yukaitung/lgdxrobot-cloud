using System.ComponentModel.DataAnnotations;

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
