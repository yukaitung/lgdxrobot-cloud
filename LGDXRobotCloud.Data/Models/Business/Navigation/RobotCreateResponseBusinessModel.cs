using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record RobotCreateResponseBusinessModel
{
  public required Guid RobotId { get; set; }

  public required string RobotName { get; set; }

  public required string RootCertificate { get; set; }

  public required string RobotCertificatePrivateKey { get; set; }

  public required string RobotCertificatePublicKey { get; set; }
}

public static class RobotCreateResponseBusinessModelExtensions
{
  public static RobotCertificateIssueDto ToDto(this RobotCreateResponseBusinessModel model)
  {
    return new RobotCertificateIssueDto {
      Robot = new RobotSearchDto {
        Id = model.RobotId,
        Name = model.RobotName
      },
      RootCertificate = model.RootCertificate,
      RobotCertificatePrivateKey = model.RobotCertificatePrivateKey,
      RobotCertificatePublicKey = model.RobotCertificatePublicKey
    };
  }
}