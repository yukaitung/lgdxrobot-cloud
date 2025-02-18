namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record RobotCertificateListDto
{
  public required Guid Id { get; set; }
  
  public required string Thumbprint { get; set; }

  public string? ThumbprintBackup { get; set; }

  public required DateTime NotBefore { get; set; }

  public required DateTime NotAfter { get; set; }
}