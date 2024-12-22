using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Setting.Certificates;

public sealed partial class Certificates : ComponentBase
{
  [Inject]
  public required IRobotCertificateService RobotCertificateService { get; set; }

  private RootCertificateDto? RootCertificate { get; set; } = null!;

  public async Task GetRootCertificate()
  {
    if (RootCertificate == null)
    {
      RootCertificate = await RobotCertificateService.GetRootCertificateAsync();
    }
  }
}