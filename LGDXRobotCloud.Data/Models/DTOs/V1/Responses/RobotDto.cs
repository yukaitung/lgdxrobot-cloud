namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record RobotDto
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public required RealmSearchDto Realm { get; set; }

  public required bool IsRealtimeExchange { get; set; }

  public required bool IsProtectingHardwareSerialNumber { get; set; }

  public required RobotCertificateDto RobotCertificate { get; set; }

  public RobotSystemInfoDto? RobotSystemInfo { get; set; }

  public RobotChassisInfoDto? RobotChassisInfo { get; set; }

  public required IEnumerable<AutoTaskListDto> AssignedTasks { get; set; } = [];
}
