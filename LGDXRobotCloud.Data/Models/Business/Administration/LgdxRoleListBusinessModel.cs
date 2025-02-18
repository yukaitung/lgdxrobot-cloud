using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Administration;

public class LgdxRoleListBusinessModel
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public string? Description { get; set; }
}

public static class LgdxRoleListBusinessModelExtensions
{
  public static LgdxRoleListDto ToDto(this LgdxRoleListBusinessModel model)
  {
    return new LgdxRoleListDto
    {
      Id = model.Id,
      Name = model.Name,
      Description = model.Description,
    };
  }

  public static IEnumerable<LgdxRoleListDto> ToDto(this IEnumerable<LgdxRoleListBusinessModel> models)
  {
    return models.Select(m => m.ToDto());
  }
}
