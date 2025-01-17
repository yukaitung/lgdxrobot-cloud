namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public record RobotCertificateIssueBusinessModel
{
  required public string RootCertificate { get; set; }
  
  required public string RobotCertificatePrivateKey { get; set; }

  required public string RobotCertificatePublicKey { get; set; }

  required public string RobotCertificateThumbprint { get; set; }

  required public DateTime RobotCertificateNotBefore { get; set; }

  required public DateTime RobotCertificateNotAfter { get; set; }
}