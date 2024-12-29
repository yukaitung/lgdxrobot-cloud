namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public class RobotCertificateIssueDto
{
  public Guid? RobotId { get; set; }

  public string? RobotName { get; set; }

  public required string RootCertificate { get; set; }

  public required string RobotCertificatePrivateKey { get; set; }

  public required string RobotCertificatePublicKey { get; set; }
}