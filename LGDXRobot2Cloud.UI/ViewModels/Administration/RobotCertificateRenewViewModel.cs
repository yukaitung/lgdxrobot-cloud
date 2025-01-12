using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Administration;

public class RobotCertificateRenewViewModel : FormViewModel
{
  public bool RevokeOldCertificate { get; set; }
}

public static class RobotCertificateRenewViewModelExtensions
{
  public static RobotCertificateRenewRequestDto ToDto(this RobotCertificateRenewViewModel robotCertificateRenewViewModel)
  {
    return new RobotCertificateRenewRequestDto {
      RevokeOldCertificate = robotCertificateRenewViewModel.RevokeOldCertificate
    };
  }
}