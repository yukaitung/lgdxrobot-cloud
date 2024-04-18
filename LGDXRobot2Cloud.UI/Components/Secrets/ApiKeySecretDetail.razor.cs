using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Secrets
{
  public partial class ApiKeySecretDetail
  {
    [Inject]
    public required IApiKeyService ApiKeyService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Parameter]
    public int? Id { get; set; }

    [Parameter]
    public bool IsThirdParty { get; set; }

    [Parameter]
    public EventCallback<(int, string, CrudOperation)> OnSubmitDone { get; set; }

    private ApiKeySecretDto? _getApiKeySecret { get; set; } = null;
    private ApiKeySecretDto _updateApiKeySecret { get; set; } = new ApiKeySecretDto();
    private bool _isError { get; set; } = false;

    private async Task HandleGetSecret()
    {
      if (Id != null)
      {
        _getApiKeySecret = await ApiKeyService.GetApiKeySecretAsync((int)Id);
      }
    }

    private async Task HandleSubmit()
    {

    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
      {
        _isError = false;
        _updateApiKeySecret = new ApiKeySecretDto();
        _getApiKeySecret = null;
      }
      await base.SetParametersAsync(parameters);
    }
  }
}