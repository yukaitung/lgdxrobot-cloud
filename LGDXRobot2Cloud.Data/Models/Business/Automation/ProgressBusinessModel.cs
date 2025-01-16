using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record ProgressBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required bool System { get; set; }

  public required bool Reserved { get; set; }
}

public static class ProgressBusinessModelExtensions
{
  public static ProgressDto ToDto(this ProgressBusinessModel progressBusinessModel)
  {
    return new ProgressDto {
      Id = progressBusinessModel.Id,
      Name = progressBusinessModel.Name,
      System = progressBusinessModel.System,
      Reserved = progressBusinessModel.Reserved,
    };
  }

  public static IEnumerable<ProgressDto> ToDto(this IEnumerable<ProgressBusinessModel> progressBusinessModels)
  {
    return progressBusinessModels.Select(p => p.ToDto());
  }
}