using AutoMapper;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Flows
{
  public partial class FlowDetail : AbstractForm
  {
    [Inject]
    public required IFlowService FlowService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }

    [Parameter]
    public int? Id { get; set; }

    [Parameter]
    public EventCallback<(int, string, CrudOperation)> OnSubmitDone { get; set; }

    private FlowBlazor Flow { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    protected override async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        // Update
        bool success = await FlowService.UpdateFlowAsync((int)Id, Mapper.Map<FlowUpdateDto>(Flow));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "flowDetailModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Flow.Name, CrudOperation.Update));
        }
        else
          IsError = true;
      }
      else
      {
        // Create
        var success = await FlowService.AddFlowAsync(Mapper.Map<FlowCreateDto>(Flow));
        if (success != null)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "flowDetailModal");
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
        var success = await FlowService.DeleteFlowAsync((int)Id);
        if (success)
        {
          // DO NOT REVERSE THE ORDER
          await JSRuntime.InvokeVoidAsync("CloseModal", "flowDeleteModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Flow.Name, CrudOperation.Delete));
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
          var flow = await FlowService.GetFlowAsync((int)_id);
          if (flow != null) {
            Flow = flow;
            _editContext = new EditContext(Flow);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          }
        }
        else
        {
          Flow = new FlowBlazor();
          _editContext = new EditContext(Flow);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      await base.SetParametersAsync(ParameterView.Empty);
    }
  }
}