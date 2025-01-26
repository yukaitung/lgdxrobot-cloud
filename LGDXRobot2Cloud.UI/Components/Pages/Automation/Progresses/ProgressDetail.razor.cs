using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.ViewModels.Automation;
using LGDXRobot2Cloud.UI.ViewModels.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.Progresses;

public sealed partial class ProgressDetail : ComponentBase
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private ProgressDetailViewModel ProgressDetailViewModel { get; set; } = new();
  private DeleteEntryModalViewModel DeleteEntryModalViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleValidSubmit()
  {
    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Automation.Progresses[ProgressDetailViewModel.Id].PutAsync(ProgressDetailViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Automation.Progresses.PostAsync(ProgressDetailViewModel.ToCreateDto());
      }
      NavigationManager.NavigateTo(AppRoutes.Automation.Progresses.Index);
    }
    catch (ApiException ex)
    {
      ProgressDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleTestDelete()
  {
    DeleteEntryModalViewModel.Errors = null;
    try
    {
      await LgdxApiClient.Automation.Progresses[ProgressDetailViewModel.Id].TestDelete.PostAsync();
      DeleteEntryModalViewModel.IsReady = true;
    }
    catch (ApiException ex)
    {
      DeleteEntryModalViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Automation.Progresses[ProgressDetailViewModel.Id].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Automation.Progresses.Index);
    }
    catch (ApiException ex)
    {
      ProgressDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var progress = await LgdxApiClient.Automation.Progresses[(int)_id].GetAsync();
        ProgressDetailViewModel.FromDto(progress!);
        _editContext = new EditContext(ProgressDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        _editContext = new EditContext(ProgressDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}