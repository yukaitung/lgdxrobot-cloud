using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record ProgressSearchBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }
}

public static class ProgressSearchBusinessModelExtensions
{
  public static ProgressSearchDto ToDto(this ProgressSearchBusinessModel progressSearchBusinessModel)
  {
    return new ProgressSearchDto {
      Id = progressSearchBusinessModel.Id,
      Name = progressSearchBusinessModel.Name,
    };
  }

  public static IEnumerable<ProgressSearchDto> ToDto(this IEnumerable<ProgressSearchBusinessModel> progressSearchBusinessModels)
  {
    return progressSearchBusinessModels.Select(p => p.ToDto());
  }
}