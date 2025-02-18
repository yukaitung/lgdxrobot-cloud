using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Administration;

public record RobotCertificateBusinessModel
{
  public required Guid Id { get; set; }

  public required Guid RobotId { get; set; }

  public required string RobotName { get; set; }
  
  public required string Thumbprint { get; set; }

  public string? ThumbprintBackup { get; set; }

  public required DateTime NotBefore { get; set; }

  public required DateTime NotAfter { get; set; }
}

public static class RobotCertificateBusinessModelExtensions
{
  public static RobotCertificateDto ToDto(this RobotCertificateBusinessModel robotCertificate)
  {
    return new RobotCertificateDto{
      Id = robotCertificate.Id,
      Robot = new RobotSearchDto {
        Id = robotCertificate.RobotId,
        Name = robotCertificate.RobotName
      },
      Thumbprint = robotCertificate.Thumbprint,
      ThumbprintBackup = robotCertificate.ThumbprintBackup,
      NotBefore = robotCertificate.NotBefore,
      NotAfter = robotCertificate.NotAfter
    };
  }
}