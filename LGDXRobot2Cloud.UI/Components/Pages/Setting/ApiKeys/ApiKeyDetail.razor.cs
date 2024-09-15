using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Setting.ApiKeys;

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

  private ApiKey ApiKey { get; set; } = null!;
  private ApiKeySecret? GetApiKeySecret { get; set; } = null;
  private ApiKeySecret UpdateApiKeySecret { get; set; } = null!;
  private EditContext _editContext = null!;
  private EditContext _editContextSecret = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  // Form
  public void HandleApiKeyKindChanged(object args)
  {
    if (bool.TryParse(args.ToString(), out bool result))      
      ApiKey.IsThirdParty = result;
  }

  public async Task HandleValidSubmit()
  {
    bool success;

    if (Id != null)
      // Update
      success = await ApiKeyService.UpdateApiKeyAsync((int)Id, Mapper.Map<ApiKeyUpdateDto>(ApiKey));
    else
      // Create
      success = await ApiKeyService.AddApiKeyAsync(Mapper.Map<ApiKeyCreateDto>(ApiKey));

    if (success)
      NavigationManager.NavigateTo(AppRoutes.Setting.ApiKeys.Index);
    else
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      bool success = await ApiKeyService.DeleteApiKeyAsync((int)Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Setting.ApiKeys.Index);
      else
        IsError = true;
    }
  }

  public async Task HandleGetSecret()
  {
    if (Id != null)
    {
      GetApiKeySecret = await ApiKeyService.GetApiKeySecretAsync((int)Id);
    }
  }

  public async Task HandleSetSecret()
  {
    if (Id != null)
      {
        bool success = await ApiKeyService.UpdateApiKeySecretAsync((int)Id, Mapper.Map<ApiKeySecretDto>(UpdateApiKeySecret));
        if (success)
          NavigationManager.NavigateTo(AppRoutes.Setting.ApiKeys.Index);
        else
          IsError = true;
      }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var apiKey = await ApiKeyService.GetApiKeyAsync((int)_id);
        if (apiKey != null) 
        {
          ApiKey = apiKey;
          ApiKey.IsUpdate = true;
          _editContext = new EditContext(ApiKey);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);

          UpdateApiKeySecret = new ApiKeySecret
          {
            IsThirdParty = apiKey.IsThirdParty
          };
          _editContextSecret = new EditContext(UpdateApiKeySecret);
          _editContextSecret.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        ApiKey = new ApiKey();
        _editContext = new EditContext(ApiKey);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}
