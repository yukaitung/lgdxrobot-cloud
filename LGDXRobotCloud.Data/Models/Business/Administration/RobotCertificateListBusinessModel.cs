using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Administration;

public record RobotCertificateListBusinessModel
{
  public required Guid Id { get; set; }
  
  public required string Thumbprint { get; set; }

  public string? ThumbprintBackup { get; set; }

  public required DateTime NotBefore { get; set; }

  public required DateTime NotAfter { get; set; }
}

public static class RobotCertificateListBusinessModelExtensions
{
  public static RobotCertificateListDto ToDto(this RobotCertificateListBusinessModel robotCertificate)
  {
    return new RobotCertificateListDto{
      Id = robotCertificate.Id,
      Thumbprint = robotCertificate.Thumbprint,
      ThumbprintBackup = robotCertificate.ThumbprintBackup,
      NotBefore = robotCertificate.NotBefore,
      NotAfter = robotCertificate.NotAfter
    };
  }

  public static IEnumerable<RobotCertificateListDto> ToDto(this IEnumerable<RobotCertificateListBusinessModel> robotCertificates)
  {
    return robotCertificates.Select(a => a.ToDto());
  }
}