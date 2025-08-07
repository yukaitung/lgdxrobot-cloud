using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using LGDXRobotCloud.UI.Client;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Realms.Components;

public sealed partial class SoftwareEmergencyStopModel
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public int RealmId { get; set; }

  [Parameter]
  public bool SoftwareEmergencyStop { get; set; }

  public async Task HandleRequest()
  {
    bool newValue = !SoftwareEmergencyStop;
    await LgdxApiClient.Navigation.Realms[RealmId].Slam.EmergencyStop.PostAsync(new() {
      Enable = newValue
    });
    await JSRuntime.InvokeVoidAsync("CloseModal", "#softwareEmergencyStop");
  }
}