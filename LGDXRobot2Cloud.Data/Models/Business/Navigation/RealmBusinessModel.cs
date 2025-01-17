using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Navigation;

public record RealmBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public string? Description { get; set; }

  public required string Image { get; set; }

  public required double Resolution { get; set; }

  public required double OriginX { get; set; }

  public required double OriginY { get; set; }

  public required double OriginRotation { get; set; }
}

public static class RealmBusinessModelExtensions
{
  public static RealmDto ToDto(this RealmBusinessModel model)
  {
    return new RealmDto {
      Id = model.Id,
      Name = model.Name,
      Description = model.Description,
      Image = model.Image,
      Resolution = model.Resolution,
      OriginX = model.OriginX,
      OriginY = model.OriginY,
      OriginRotation = model.OriginRotation,
    };
  }
}