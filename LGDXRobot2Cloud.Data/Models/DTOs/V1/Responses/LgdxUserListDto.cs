namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record LgdxUserListDto
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public required string UserName { get; set; }

  public required bool TwoFactorEnabled { get; set; }

  public required int AccessFailedCount { get; set; }
}