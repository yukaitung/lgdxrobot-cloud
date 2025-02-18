using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Automation.TriggerRetries;

public sealed partial class TriggerRetryDetail : ComponentBase
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private Dictionary<string, string>? Errors { get; set; } = null;

  private TriggerRetryDto TriggerRetry { get; set; } = null!;

  public async Task HandleRetry()
  {
    try
    {
      await LgdxApiClient.Automation.TriggerRetries[(int)Id!].Retry.PostAsync();
      NavigationManager.NavigateTo(AppRoutes.Automation.TriggerRetries.Index);
    }
    catch (ApiException ex)
    {
      Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Automation.TriggerRetries[(int)Id!].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Automation.TriggerRetries.Index);
    }
    catch (ApiException ex)
    {
      Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
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