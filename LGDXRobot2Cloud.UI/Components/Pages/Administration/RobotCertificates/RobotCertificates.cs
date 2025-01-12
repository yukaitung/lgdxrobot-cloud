using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Administration.RobotCertificates;

public sealed partial class RobotCertificates : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  private RootCertificateDto? RootCertificate { get; set; } = null;

  public async Task GetRootCertificate()
  {
    RootCertificate ??= await LgdxApiClient.Administration.RobotCertificates.Root.GetAsync();
  }
}