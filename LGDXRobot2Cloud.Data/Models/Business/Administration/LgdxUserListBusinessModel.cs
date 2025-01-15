using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public record LgdxUserListBusinessModel
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public required string UserName { get; set; }

  public required bool TwoFactorEnabled { get; set; }

  public required int AccessFailedCount { get; set; }
}

public static class LgdxUserListBusinessModelExtensions
{
  public static LgdxUserListDto ToDto(this LgdxUserListBusinessModel model)
  {
    return new LgdxUserListDto {
      Id = model.Id,
      Name = model.Name,
      UserName = model.UserName,
      TwoFactorEnabled = model.TwoFactorEnabled,
      AccessFailedCount = model.AccessFailedCount,
    };
  }

  public static IEnumerable<LgdxUserListDto> ToDto(this IEnumerable<LgdxUserListBusinessModel> model)
  {
    return model.Select(m => m.ToDto());
  }
}