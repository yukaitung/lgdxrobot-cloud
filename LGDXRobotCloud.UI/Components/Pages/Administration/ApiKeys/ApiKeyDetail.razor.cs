using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.ApiKeys;

public partial class ApiKeyDetail
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private ApiKeyDetailViewModel ApiKeyDetailViewModel { get; set; } = new();
  private ApiKeyDetailSectretViewModel UpdateApiKeySecretViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private EditContext _editContextSecret = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  // Form
  public void HandleApiKeyKindChanged(object args)
  {
    if (bool.TryParse(args.ToString(), out bool result))      
      ApiKeyDetailViewModel.IsThirdParty = result;
  }

  public async Task HandleValidSubmit()
  {
    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Administration.ApiKeys[ApiKeyDetailViewModel.Id].PutAsync(ApiKeyDetailViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Administration.ApiKeys.PostAsync(ApiKeyDetailViewModel.ToCreateDto());
      }
    }
    catch (ApiException ex)
    {
      ApiKeyDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    NavigationManager.NavigateTo(AppRoutes.Administration.ApiKeys.Index);
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Administration.ApiKeys[ApiKeyDetailViewModel.Id].DeleteAsync();
    }
    catch (ApiException ex)
    {
      ApiKeyDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    NavigationManager.NavigateTo(AppRoutes.Administration.ApiKeys.Index);
  }

  public async Task HandleGetSecret()
  {
    try
    {
      var response = await LgdxApiClient.Administration.ApiKeys[ApiKeyDetailViewModel.Id].Secret.GetAsync();
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
        await LgdxApiClient.Administration.ApiKeys[ApiKeyDetailViewModel.Id].Secret.PutAsync(new ApiKeySecretUpdateDto { Secret = UpdateApiKeySecretViewModel.UpdateSecret });
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
      ApiKeyDetailViewModel.FromDto(apiKey!);
      _editContext = new EditContext(ApiKeyDetailViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      _editContextSecret = new EditContext(UpdateApiKeySecretViewModel);
      _editContextSecret.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    else
    {
      _editContext = new EditContext(ApiKeyDetailViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      _editContextSecret = new EditContext(UpdateApiKeySecretViewModel);
      _editContextSecret.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}