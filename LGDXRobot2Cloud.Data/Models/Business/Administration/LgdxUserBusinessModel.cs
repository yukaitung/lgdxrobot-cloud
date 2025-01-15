using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public record LgdxUserBusinessModel
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public required string UserName { get; set; }

  public required string Email { get; set; }

  public IEnumerable<string> Roles { get; set; } = [];

  public required bool TwoFactorEnabled { get; set; }

  public required int AccessFailedCount { get; set; }
}

public static class LgdxUserBusinessModelExtensions
{
  public static LgdxUserDto ToDto(this LgdxUserBusinessModel model)
  {
    return new LgdxUserDto {
      Id = model.Id,
      Name = model.Name,
      UserName = model.UserName,
      Email = model.Email,
      Roles = model.Roles,
      TwoFactorEnabled = model.TwoFactorEnabled,
      AccessFailedCount = model.AccessFailedCount,
    };
  }
}