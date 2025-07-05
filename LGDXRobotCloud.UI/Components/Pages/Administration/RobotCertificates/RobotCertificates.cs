using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.RobotCertificates;

public sealed partial class RobotCertificates : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  private RootCertificateDto? RootCertificate { get; set; } = null;
  TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

  public async Task GetRootCertificate()
  {
    RootCertificate ??= await LgdxApiClient.Administration.RobotCertificates.Root.GetAsync();
  }

  protected override void OnInitialized()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    TimeZone = settings.TimeZone;
    OnInitializedAsync();
  }
}