using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Constants;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.TriggerRetries;

public sealed partial class TriggerRetryDetail : ComponentBase
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private TriggerRetryDto TriggerRetry { get; set; } = null!;

  public async Task HandleRetry()
  {
    await LgdxApiClient.Automation.TriggerRetries[(int)Id!].Retry.PostAsync();
    NavigationManager.NavigateTo(AppRoutes.Automation.TriggerRetries.Index);
  }

  public async Task HandleDelete()
  {
    await LgdxApiClient.Automation.TriggerRetries[(int)Id!].DeleteAsync();
    NavigationManager.NavigateTo(AppRoutes.Automation.TriggerRetries.Index);
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var triggerRetry = await LgdxApiClient.Automation.TriggerRetries[(int)_id].GetAsync();
        TriggerRetry = triggerRetry!;
      }
      else
      {
        NavigationManager.NavigateTo(AppRoutes.Home);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}