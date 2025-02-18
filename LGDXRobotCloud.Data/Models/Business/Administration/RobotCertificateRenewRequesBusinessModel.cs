namespace LGDXRobotCloud.Data.Models.Business.Administration;

public record RobotCertificateRenewRequestBusinessModel
{
  public required Guid CertificateId { get; set; }

  public bool RevokeOldCertificate { get; set; }
}