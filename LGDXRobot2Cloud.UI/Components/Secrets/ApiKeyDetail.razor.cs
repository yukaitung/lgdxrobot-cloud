using AutoMapper;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Secrets
{
  public partial class ApiKeyDetail : AbstractForm
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
    public EventCallback<(int, string, CrudOperation)> OnSubmitDone { get; set; }

    private ApiKey ApiKey { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    // Form
    void HandleApiKeyKindChanged(object args)
    {
      if (bool.TryParse(args.ToString(), out bool result))      
        ApiKey.IsThirdParty = result;
    }

    protected override async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        // Update
        bool success = await ApiKeyService.UpdateApiKeyAsync((int)Id, Mapper.Map<ApiKeyUpdateDto>(ApiKey));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "apiKeyDetailModal");
          await OnSubmitDone.InvokeAsync(((int)Id, ApiKey.Name, CrudOperation.Update));
        }
        else
          IsError = true;
      }
      else
      {
        // Create
        var success = await ApiKeyService.AddApiKeyAsync(Mapper.Map<ApiKeyCreateDto>(ApiKey));
        if (success != null)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "apiKeyDetailModal");
          await OnSubmitDone.InvokeAsync((success.Id, success.Name, CrudOperation.Create));
        }
        else
          IsError = true;
      }
    }

    protected override void HandleInvalidSubmit()
    {
      IsInvalid = true;
    }

    protected override async void HandleDelete()
    {
      if (Id != null)
      {
        var success = await ApiKeyService.DeleteApiKeyAsync((int)Id);
        if (success)
        {
          // DO NOT REVERSE THE ORDER
          await JSRuntime.InvokeVoidAsync("CloseModal", "apiKeyDeleteModal");
          await OnSubmitDone.InvokeAsync(((int)Id, ApiKey.Name, CrudOperation.Delete));
        } 
        else
          IsError = true;
      }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      parameters.SetParameterProperties(this);
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
      {
        IsInvalid = false;
        IsError = false;
        if (_id != null)
        {
          var apiKey = await ApiKeyService.GetApiKeyAsync((int)_id);
          if (apiKey != null) {
            ApiKey = apiKey;
            _editContext = new EditContext(ApiKey);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
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
}