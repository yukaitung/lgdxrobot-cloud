using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public class LgdxRoleBusinessModel
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public string? Description { get; set; }

  public IEnumerable<string> Scopes { get; set; } = [];
}

public static class LgdxRoleBusinessModelExtensions
{
  public static LgdxRoleDto ToDto(this LgdxRoleBusinessModel model)
  {
    return new LgdxRoleDto
    {
      Id = model.Id,
      Name = model.Name,
      Description = model.Description,
      Scopes = model.Scopes,
    };
  }

}