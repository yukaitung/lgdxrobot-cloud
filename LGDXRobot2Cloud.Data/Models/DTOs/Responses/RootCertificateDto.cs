namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public record RootCertificateDto
{
  public string PublicKey { get; set; } = null!;
}