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

    private ApiKey _apiKey { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool _isInvalid { get; set; } = false;
    private bool _isError { get; set; } = false;

    // Form
    void HandleApiKeyKindChanged(object args)
    {
      bool.TryParse(args.ToString(), out var result);        
      _apiKey.IsThirdParty = result;
    }

    protected override async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        // Update
        bool success = await ApiKeyService.UpdateApiKeyAsync((int)Id, Mapper.Map<ApiKeyUpdateDto>(_apiKey));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "apiKeyDetailModal");
          await OnSubmitDone.InvokeAsync(((int)Id, _apiKey.Name, CrudOperation.Update));
        }
        else
          _isError = true;
      }
      else
      {
        // Create
        var success = await ApiKeyService.AddApiKeyAsync(Mapper.Map<ApiKeyCreateDto>(_apiKey));
        if (success != null)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "apiKeyDetailModal");
          await OnSubmitDone.InvokeAsync((success.Id, success.Name, CrudOperation.Create));
        }
        else
          _isError = true;
      }
    }

    protected override void HandleInvalidSubmit()
    {
      _isInvalid = true;
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
          await OnSubmitDone.InvokeAsync(((int)Id, _apiKey.Name, CrudOperation.Delete));
        } 
        else
          _isError = true;
      }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      parameters.SetParameterProperties(this);
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
      {
        _isInvalid = false;
        _isError = false;
        if (_id != null)
        {
          var apiKey = await ApiKeyService.GetApiKeyAsync((int)_id);
          if (apiKey != null) {
            _apiKey = apiKey;
            _editContext = new EditContext(_apiKey);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          }
        }
        else
        {
          _apiKey = new ApiKey();
          _editContext = new EditContext(_apiKey);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      await base.SetParametersAsync(ParameterView.Empty);
    }
  }
}