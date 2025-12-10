using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Administration;

public class RobotCertificateRenewViewModel : FormViewModelBase
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