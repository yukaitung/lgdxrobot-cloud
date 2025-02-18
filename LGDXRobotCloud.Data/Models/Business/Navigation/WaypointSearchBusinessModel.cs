using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record WaypointSearchBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; } = null!;
}

public static class WaypointSearchBusinessModelExtensions
{
  public static WaypointSearchDto ToDto(this WaypointSearchBusinessModel model)
  {
    return new WaypointSearchDto {
      Id = model.Id,
      Name = model.Name,
    };
  }
  
  public static IEnumerable<WaypointSearchDto> ToDto(this IEnumerable<WaypointSearchBusinessModel> models)
  {
    return models.Select(model => model.ToDto());
  }
}