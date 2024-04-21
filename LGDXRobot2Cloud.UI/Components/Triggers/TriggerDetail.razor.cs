using AutoMapper;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Triggers
{
  public partial class TriggerDetail : AbstractForm
  {
    [Inject]
    public required ITriggerService TriggerService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }

    [Parameter]
    public int? Id { get; set; }

    [Parameter]
    public EventCallback<(int, string, CrudOperation)> OnSubmitDone { get; set; }

    private TriggerBlazor Trigger { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    // Form
    private void HandleApiKeyInsertAt(object args)
    {
      Trigger.ApiKeyInsertAt = args.ToString();
    }

    protected override async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        // Update
        bool success = await TriggerService.UpdateTriggerAsync((int)Id, Mapper.Map<TriggerUpdateDto>(Trigger));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "triggerDetailModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Trigger.Name, CrudOperation.Update));
        }
        else
          IsError = true;
      }
      else
      {
        // Create
        var success = await TriggerService.AddTriggerAsync(Mapper.Map<TriggerCreateDto>(Trigger));
        if (success != null)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "triggerDetailModal");
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
        var success = await TriggerService.DeleteTriggerAsync((int)Id);
        if (success)
        {
          // DO NOT REVERSE THE ORDER
          await JSRuntime.InvokeVoidAsync("CloseModal", "triggerDeleteModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Trigger.Name, CrudOperation.Delete));
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
          var node = await TriggerService.GetTriggerAsync((int)_id);
          if (node != null)
          {
            Trigger = node;
            _editContext = new EditContext(Trigger);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          }
        }
        else
        {
          Trigger = new TriggerBlazor
          {
            ApiKeyInsertAt = "header"
          };
          _editContext = new EditContext(Trigger);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      await base.SetParametersAsync(ParameterView.Empty);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (Trigger.ApiKeyRequired)
      {
        await JSRuntime.InvokeVoidAsync("InitAdvancedSelect", "ApiKeySelect");
      }
      await base.OnAfterRenderAsync(firstRender);
    }
  }
}