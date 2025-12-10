using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.ApiKeys;

public partial class ApiKeyDetails
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private ApiKeyDetailsViewModel ApiKeyDetailsViewModel { get; set; } = new();
  private ApiKeyDetailsSectretViewModel UpdateApiKeySecretViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private EditContext _editContextSecret = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  // Form
  public void HandleApiKeyKindChanged(object args)
  {
    if (bool.TryParse(args.ToString(), out bool result))      
      ApiKeyDetailsViewModel.IsThirdParty = result;
  }

  public async Task HandleValidSubmit()
  {
    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Administration.ApiKeys[ApiKeyDetailsViewModel.Id].PutAsync(ApiKeyDetailsViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Administration.ApiKeys.PostAsync(ApiKeyDetailsViewModel.ToCreateDto());
      }
    }
    catch (ApiException ex)
    {
      ApiKeyDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    NavigationManager.NavigateTo(AppRoutes.Administration.ApiKeys.Index);
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Administration.ApiKeys[ApiKeyDetailsViewModel.Id].DeleteAsync();
    }
    catch (ApiException ex)
    {
      ApiKeyDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    NavigationManager.NavigateTo(AppRoutes.Administration.ApiKeys.Index);
  }

  public async Task HandleGetSecret()
  {
    try
    {
      var response = await LgdxApiClient.Administration.ApiKeys[ApiKeyDetailsViewModel.Id].Secret.GetAsync();
      UpdateApiKeySecretViewModel.Secret = response!.Secret;
    }
    catch (ApiException ex)
    {
      UpdateApiKeySecretViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleSetSecret()
  {
    try
    {
      if (Id != null)
      {
        await LgdxApiClient.Administration.ApiKeys[ApiKeyDetailsViewModel.Id].Secret.PutAsync(new ApiKeySecretUpdateDto { Secret = UpdateApiKeySecretViewModel.UpdateSecret });
        NavigationManager.NavigateTo(AppRoutes.Administration.ApiKeys.Index);
      }
    }
    catch (ApiException ex)
    {
      UpdateApiKeySecretViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id) && _id != null)
    {
      var apiKey = await LgdxApiClient.Administration.ApiKeys[(int)_id].GetAsync();
      ApiKeyDetailsViewModel.FromDto(apiKey!);
      _editContext = new EditContext(ApiKeyDetailsViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      _editContextSecret = new EditContext(UpdateApiKeySecretViewModel);
      _editContextSecret.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    else
    {
      _editContext = new EditContext(ApiKeyDetailsViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      _editContextSecret = new EditContext(UpdateApiKeySecretViewModel);
      _editContextSecret.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}