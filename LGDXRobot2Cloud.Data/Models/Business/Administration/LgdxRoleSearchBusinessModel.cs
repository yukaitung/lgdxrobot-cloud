using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public class LgdxRoleSearchBusinessModel
{
  public required Guid Id { get; set; } 

  public required string Name { get; set; }
}

public static class LgdxRoleSearchBusinessModelExtensions
{
  public static LgdxRoleSearchDto ToDto(this LgdxRoleSearchBusinessModel model)
  {
    return new LgdxRoleSearchDto
    {
      Id = model.Id,
      Name = model.Name,
    };
  }

  public static IEnumerable<LgdxRoleSearchDto> ToDto(this IEnumerable<LgdxRoleSearchBusinessModel> models)
  {
    return models.Select(m => m.ToDto());
  }
}