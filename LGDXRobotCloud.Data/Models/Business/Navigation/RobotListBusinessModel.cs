using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record RobotListBusinessModel
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public required int RealmId { get; set; }

  public required string RealmName { get; set; }
}

public static class RobotListBusinessModelExtensions
{
  public static RobotListDto ToDto(this RobotListBusinessModel model)
  {
    return new RobotListDto {
      Id = model.Id,
      Name = model.Name,
      Realm = new RealmSearchDto {
        Id = model.RealmId,
        Name = model.RealmName,
      },
    };
  }

  public static IEnumerable<RobotListDto> ToDto(this IEnumerable<RobotListBusinessModel> models)
  { 
    return models.Select(model => model.ToDto());
  }
}