using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace LGDXRobot2Cloud.UI.Components.Pages.Administration.RobotCertificates;

public sealed partial class RobotCertificateDetail
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ProtectedSessionStorage ProtectedSessionStorage { get; set; } = default!;

  [Parameter]
  public string? Id { get; set; }

  RobotCertificateDto? RobotCertificate { get; set; } = null!;

  string RedirectUrl { get; set; } = string.Empty;

  public async Task SetRedirectUrl()
  {
    await ProtectedSessionStorage.SetAsync("redirectUrl", RedirectUrl);
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id) && _id != null)
    {
      if (Guid.TryParse(_id, out Guid _guid))
        RobotCertificate = await LgdxApiClient.Administration.RobotCertificates[_guid].GetAsync();
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }

  protected override Task OnInitializedAsync()
  {
    RedirectUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
    return base.OnInitializedAsync();
  }
}