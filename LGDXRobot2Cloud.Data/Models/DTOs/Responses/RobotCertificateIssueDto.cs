namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class RobotCertificateIssueDto
{
  public Guid? RobotId { get; set; }

  public string? RobotName { get; set; } = null!;

  public string RootCertificate { get; set; } = null!;

  public string RobotCertificatePrivateKey { get; set; } = null!;

  public string RobotCertificatePublicKey { get; set; } = null!;
}