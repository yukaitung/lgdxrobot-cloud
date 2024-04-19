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

    private ApiKeySecretDto? GetApiKeySecret { get; set; } = null;
    private ApiKeySecretDto UpdateApiKeySecret { get; set; } = null!;
    private bool IsError { get; set; } = false;

    private async Task HandleGetSecret()
    {
      if (Id != null)
      {
        GetApiKeySecret = await ApiKeyService.GetApiKeySecretAsync((int)Id);
      }
    }

    private async Task HandleSubmit()
    {
      if (Id != null)
      {
        bool success = await ApiKeyService.UpdateApiKeySecretAsync((int)Id, UpdateApiKeySecret);
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "apiKeySecretModal");
          await OnSubmitDone.InvokeAsync(((int)Id, "", CrudOperation.Update));
        }
        else
          IsError = true;
      }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
      {
        IsError = false;
        UpdateApiKeySecret = new ApiKeySecretDto();
        GetApiKeySecret = null;
      }
      await base.SetParametersAsync(parameters);
    }
  }
}