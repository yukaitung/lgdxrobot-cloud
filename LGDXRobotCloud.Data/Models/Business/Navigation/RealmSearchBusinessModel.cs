using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public class RealmSearchBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }
}

public static class RealmSearchBusinessModelExtensions
{
  public static RealmSearchDto ToDto(this RealmSearchBusinessModel model)
  {
    return new RealmSearchDto
    {
      Id = model.Id,
      Name = model.Name,
    };
  }
  
  public static IEnumerable<RealmSearchDto> ToDto(this IEnumerable<RealmSearchBusinessModel> models)
  {
    return models.Select(model => model.ToDto());
  }
}