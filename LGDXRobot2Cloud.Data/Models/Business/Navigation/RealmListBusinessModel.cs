using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Navigation;

public record RealmListBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public string? Description { get; set; }

  public required double Resolution { get; set; }
}

public static class RealmListBusinessModelExtensions
{
  public static RealmListDto ToDto(this RealmListBusinessModel model)
  {
    return new RealmListDto {
      Id = model.Id,
      Name = model.Name,
      Description = model.Description,
      Resolution = model.Resolution,
    };
  }

  public static IEnumerable<RealmListDto> ToDto(this IEnumerable<RealmListBusinessModel> models)
  { 
    return models.Select(model => model.ToDto());
  }
}