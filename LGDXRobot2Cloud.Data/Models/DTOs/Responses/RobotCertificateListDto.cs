namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class RobotCertificateListDto
{
  public Guid Id { get; set; }
  
  public string Thumbprint { get; set; } = null!;

  public string? ThumbprintBackup { get; set; }

  public DateTime NotBefore { get; set; }

  public DateTime NotAfter { get; set; }
}