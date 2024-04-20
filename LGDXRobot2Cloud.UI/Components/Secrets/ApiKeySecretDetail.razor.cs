using AutoMapper;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Secrets
{
  public partial class ApiKeySecretDetail
  {
    [Inject]
    public required IApiKeyService ApiKeyService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }


    [Parameter]
    public int? Id { get; set; }

    [Parameter]
    public bool IsThirdParty { get; set; }

    [Parameter]
    public EventCallback<(int, string, CrudOperation)> OnSubmitDone { get; set; }

    private ApiKeySecretBlazor? GetApiKeySecret { get; set; } = null;
    private ApiKeySecretBlazor UpdateApiKeySecret { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    private async Task HandleGetSecret()
    {
      if (Id != null)
      {
        GetApiKeySecret = await ApiKeyService.GetApiKeySecretAsync((int)Id);
      }
    }

    protected async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        bool success = await ApiKeyService.UpdateApiKeySecretAsync((int)Id, Mapper.Map<ApiKeySecretDto>(UpdateApiKeySecret));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "apiKeySecretModal");
          await OnSubmitDone.InvokeAsync(((int)Id, "", CrudOperation.Update));
        }
        else
          IsError = true;
      }
    }

    protected void HandleInvalidSubmit()
    {
      IsInvalid = true;
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
      {
        IsError = false;
        GetApiKeySecret = null;
      }
      if (parameters.TryGetValue<bool?>(nameof(IsThirdParty), out var _isThirdParty))
      {
        IsInvalid = false;
        UpdateApiKeySecret = new ApiKeySecretBlazor
        {
            IsThirdParty = _isThirdParty ?? false,
        };
        _editContext = new EditContext(UpdateApiKeySecret);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      await base.SetParametersAsync(parameters);
    }
  }
}