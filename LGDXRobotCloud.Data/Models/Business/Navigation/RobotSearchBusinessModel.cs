using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record RobotSearchBusinessModel
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }
}

public static class RobotSearchBusinessModelExtensions
{
  public static RobotSearchDto ToDto(this RobotSearchBusinessModel model)
  {
    return new RobotSearchDto {
      Id = model.Id,
      Name = model.Name,
    };
  }

  public static IEnumerable<RobotSearchDto> ToDto(this IEnumerable<RobotSearchBusinessModel> models)
  {
    return models.Select(model => model.ToDto());
  }
}