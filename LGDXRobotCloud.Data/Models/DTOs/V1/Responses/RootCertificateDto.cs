namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record RootCertificateDto
{
  public required DateTime NotBefore { get; set; }

  public required DateTime NotAfter { get; set; }
  
  public required string PublicKey { get; set; }
}