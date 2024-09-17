namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class RobotCreateResponseDto
{
  public Guid Id { get; set; }

  public string Name { get; set; } = null!;

  public RobotCertificateIssueDto Certificate { get; set; } = null!;
}
