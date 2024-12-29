namespace LGDXRobot2Cloud.UI.ViewModels.Navigation;

public sealed class RobotCertificateViewModel
{
  public Guid Id { get; set; }
  
  public string Thumbprint { get; set; } = null!;

  public string? ThumbprintBackup { get; set; }

  public DateTime NotBefore { get; set; }

  public DateTime NotAfter { get; set; }
}