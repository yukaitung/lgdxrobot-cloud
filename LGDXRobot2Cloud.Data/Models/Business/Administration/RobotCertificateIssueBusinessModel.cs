using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public record RobotCertificateIssueBusinessModel
{
  public required Guid RobotId { get; set; }

  public required string RobotName { get; set; }

  public required string RootCertificate { get; set; }

  public required string RobotCertificatePrivateKey { get; set; }

  public required string RobotCertificatePublicKey { get; set; }
}

public static class RobotCertificateIssueBusinessModelExtensions
{
  public static RobotCertificateIssueDto ToDto(this RobotCertificateIssueBusinessModel robotCertificateIssue)
  {
    return new RobotCertificateIssueDto{
      Robot = new RobotSearchDto {
        Id = robotCertificateIssue.RobotId,
        Name = robotCertificateIssue.RobotName
      },
      RootCertificate = robotCertificateIssue.RootCertificate,
      RobotCertificatePrivateKey = robotCertificateIssue.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = robotCertificateIssue.RobotCertificatePublicKey
    };
  }
}