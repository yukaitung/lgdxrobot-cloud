using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.TriggerRetries;

public sealed partial class TriggerRetryDetail : ComponentBase
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required ITriggerRetryService TriggerRetryService { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private TriggerRetryDto TriggerRetry { get; set; } = null!;

  public IDictionary<string,string[]>? Errors { get; set; }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await TriggerRetryService.DeleteTriggerRetryAsync((int)Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Automation.TriggerRetries.Index);
      else
        Errors = response.Errors;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var response = await TriggerRetryService.GetTriggerRetryAsync((int)_id);
        var triggerRetry = response.Data;
        if (triggerRetry != null) 
        {
          TriggerRetry = triggerRetry;
        }
      }
      else
      {
        NavigationManager.NavigateTo(AppRoutes.Home);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}