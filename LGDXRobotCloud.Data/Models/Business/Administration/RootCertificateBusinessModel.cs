using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Administration;

public record RootCertificateBusinessModel
{
  public required DateTime NotBefore { get; set; }

  public required DateTime NotAfter { get; set; }

  public required string PublicKey { get; set; }
}

public static class RootCertificateBusinessModelExtensions
{
  public static RootCertificateDto ToDto(this RootCertificateBusinessModel rootCertificateBusinessModel)
  {
    return new RootCertificateDto {
      NotBefore = rootCertificateBusinessModel.NotBefore,
      NotAfter = rootCertificateBusinessModel.NotAfter,
      PublicKey = rootCertificateBusinessModel.PublicKey
    };
  }
}