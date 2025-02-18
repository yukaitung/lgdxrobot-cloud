using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using Microsoft.AspNetCore.Components;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.RobotCertificates;

public sealed partial class RobotCertificateDetail
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public string? Id { get; set; }

  RobotCertificateDto? RobotCertificate { get; set; } = null!;

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
}