using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Administration.ApiKeys;

public sealed partial class ApiKeyDetail
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IApiKeyService ApiKeyService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private ApiKeyDetailViewModel ApiKeyDetailViewModel { get; set; } = null!;
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
    ApiResponse<bool> response;

    if (Id != null)
      // Update
      response = await ApiKeyService.UpdateApiKeyAsync((int)Id, Mapper.Map<ApiKeyUpdateDto>(ApiKeyDetailViewModel));
    else
      // Create
      response = await ApiKeyService.AddApiKeyAsync(Mapper.Map<ApiKeyCreateDto>(ApiKeyDetailViewModel));

    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Administration.ApiKeys.Index);
    else
      ApiKeyDetailViewModel.Errors = response.Errors;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await ApiKeyService.DeleteApiKeyAsync((int)Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Administration.ApiKeys.Index);
      else
        ApiKeyDetailViewModel.Errors = response.Errors;
    }
  }

  public async Task HandleGetSecret()
  {
    if (Id != null)
    {
      var response = await ApiKeyService.GetApiKeySecretAsync((int)Id);
      if (response.IsSuccess)
        UpdateApiKeySecretViewModel.Secret = response.Data!.Secret;
      else
        ApiKeyDetailViewModel.Errors = response.Errors;
    }
  }

  public async Task HandleSetSecret()
  {
    if (Id != null)
      {
        var response = await ApiKeyService.UpdateApiKeySecretAsync((int)Id, new ApiKeySecretUpdateDto { Secret = UpdateApiKeySecretViewModel.UpdateSecret });
        if (response.IsSuccess)
          NavigationManager.NavigateTo(AppRoutes.Administration.ApiKeys.Index);
        else
          UpdateApiKeySecretViewModel.Errors = response.Errors;
      }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var response = await ApiKeyService.GetApiKeyAsync((int)_id);
        var apiKey = response.Data;
        if (apiKey != null) 
        {
          ApiKeyDetailViewModel = Mapper.Map<ApiKeyDetailViewModel>(apiKey);
          _editContext = new EditContext(ApiKeyDetailViewModel);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          _editContextSecret = new EditContext(UpdateApiKeySecretViewModel);
          _editContextSecret.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        ApiKeyDetailViewModel = new ApiKeyDetailViewModel();
        _editContext = new EditContext(ApiKeyDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        _editContextSecret = new EditContext(UpdateApiKeySecretViewModel);
        _editContextSecret.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}
