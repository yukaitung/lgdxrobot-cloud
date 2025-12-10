using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Automation;
using LGDXRobotCloud.UI.ViewModels.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Automation.Progresses;

public partial class ProgressDetails : ComponentBase
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private ProgressDetailsViewModel ProgressDetailsViewModel { get; set; } = new();
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
        await LgdxApiClient.Automation.Progresses[ProgressDetailsViewModel.Id].PutAsync(ProgressDetailsViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Automation.Progresses.PostAsync(ProgressDetailsViewModel.ToCreateDto());
      }
      NavigationManager.NavigateTo(AppRoutes.Automation.Progresses.Index);
    }
    catch (ApiException ex)
    {
      ProgressDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleTestDelete()
  {
    DeleteEntryModalViewModel.Errors = null;
    try
    {
      await LgdxApiClient.Automation.Progresses[ProgressDetailsViewModel.Id].TestDelete.PostAsync();
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
      await LgdxApiClient.Automation.Progresses[ProgressDetailsViewModel.Id].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Automation.Progresses.Index);
    }
    catch (ApiException ex)
    {
      ProgressDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
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
        ProgressDetailsViewModel.FromDto(progress!);
        _editContext = new EditContext(ProgressDetailsViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        _editContext = new EditContext(ProgressDetailsViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}