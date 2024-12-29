namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record RobotCertificateDto
{
  public Guid Id { get; set; }

  public Guid? RobotId { get; set; }

  public string? RobotName { get; set; } = null!;
  
  public string Thumbprint { get; set; } = null!;

  public string? ThumbprintBackup { get; set; }

  public DateTime NotBefore { get; set; }

  public DateTime NotAfter { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }
}