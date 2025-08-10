using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record RobotBusinessModel
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public required int RealmId { get; set; }

  public required string RealmName { get; set; }

  public required bool IsProtectingHardwareSerialNumber { get; set; }

  public required RobotCertificateBusinessModel RobotCertificate { get; set; }

  public RobotSystemInfoBusinessModel? RobotSystemInfo { get; set; }

  public RobotChassisInfoBusinessModel? RobotChassisInfo { get; set; }

  public required IEnumerable<AutoTaskListBusinessModel> AssignedTasks { get; set; } = [];
}

public static class RobotBusinessModelExtensions
{
  public static RobotDto ToDto(this RobotBusinessModel robot)
  {
    return new RobotDto {
      Id = robot.Id,
      Name = robot.Name,
      Realm = new RealmSearchDto {
        Id = robot.RealmId,
        Name = robot.RealmName,
      },
      IsProtectingHardwareSerialNumber = robot.IsProtectingHardwareSerialNumber,
      RobotCertificate = robot.RobotCertificate.ToDto(),
      RobotSystemInfo = robot.RobotSystemInfo?.ToDto(),
      RobotChassisInfo = robot.RobotChassisInfo?.ToDto(),
      AssignedTasks = robot.AssignedTasks.Select(a => a.ToDto()).ToList(),
    };
  }
}