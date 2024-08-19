namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class RobotCreateResponseDto
{
  public Guid Id { get; set; }

  public string Name { get; set; } = null!;

  public string RootCertificate { get; set; } = null!;

  public string RobotCertificatePrivateKey { get; set; } = null!;

  public string RobotCertificatePublicKey { get; set; } = null!;
}
