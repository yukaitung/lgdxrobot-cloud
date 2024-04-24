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
  public partial class FlowDetail : AbstractForm, IDisposable
  {
    [Inject]
    public required IFlowService FlowService { get; set; }

    [Inject]
    public required IProgressService ProgressService { get; set; }

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

    private DotNetObjectReference<FlowDetail> ObjectReference = null!;
    private FlowBlazor Flow { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    // Form helping variables
    private readonly string ProgressIdElement = "ProgressId-";
    private readonly string StartTriggerdElement = "StartTriggerdId-";
    private readonly string EndTriggerdElement = "EndTriggerdId-";
    private bool UpdateTomSelect { get; set; } = false;

    // Form
    [JSInvokable("HandlSelectSearch")]
    public async Task HandlSelectSearch(string elementId, string name)
    {
      if (string.IsNullOrWhiteSpace(name))
        return;
      var index = elementId.IndexOf('-');
      if (index == -1 || index + 1 == elementId.Length)
        return;
      string element = elementId[..(index + 1)];
      //int id = int.Parse(elementId[(index + 1)..]);
      if (element == ProgressIdElement)
      {
        var result = await ProgressService.SearchProgressesAsync(name);
        await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, result);
      }
      else if (element == StartTriggerdElement)
      {
        var result = await TriggerService.SearchTriggersAsync(name);
        await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, result);
      }
      else if (element == EndTriggerdElement)
      {
        var result = await TriggerService.SearchTriggersAsync(name);
        await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, result);
      }
    }

    [JSInvokable("HandleSelectChange")]
    public void HandleSelectChange(string elementId, int? id, string? name)
    {
      if (string.IsNullOrWhiteSpace(name))
        return;
      var index = elementId.IndexOf('-');
      if (index == -1 || index + 1 == elementId.Length)
        return;
      string element = elementId[..(index + 1)];
      int order = int.Parse(elementId[(index + 1)..]);
      if (element == ProgressIdElement && id != null)
      {
        Flow.FlowDetails[order].ProgressId = id;
        Flow.FlowDetails[order].ProgressName = name;
      }
      else if (element == StartTriggerdElement)
      {
        Flow.FlowDetails[order].StartTriggerId = id;
        Flow.FlowDetails[order].StartTriggerName = name;
      }
      else if (element == EndTriggerdElement)
      {
        Flow.FlowDetails[order].EndTriggerId = id;
        Flow.FlowDetails[order].EndTriggerName = name;
      }
    }


    private void HandleProceedConditionChange(int i, object? args)
    {
      if (args != null)
        Flow.FlowDetails[i].ProceedCondition = args.ToString() ?? string.Empty;
    }

    private void FlowAddStep()
    {
      Flow.FlowDetails.Add(new FlowDetailBlazor());
      UpdateTomSelect = true;
    }

    private void FlowStepMoveUp(int i)
    {
      if (i < 1)
        return;
      (Flow.FlowDetails[i], Flow.FlowDetails[i - 1]) = (Flow.FlowDetails[i - 1], Flow.FlowDetails[i]);
    }

    private void FlowStepMoveDown(int i)
    {
      if (i > Flow.FlowDetails.Count() - 1)
        return;
      (Flow.FlowDetails[i], Flow.FlowDetails[i + 1]) = (Flow.FlowDetails[i + 1], Flow.FlowDetails[i]);
    }

    private void FlowRemoveStep(int i)
    {
      if (Flow.FlowDetails.Count <= 1)
        return;
      Flow.FlowDetails.RemoveAt(i);
    }

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
          if (flow != null)
          {
            Flow = flow;
            _editContext = new EditContext(Flow);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          }
        }
        else
        {
          Flow = new FlowBlazor();
          Flow.FlowDetails.Add(new FlowDetailBlazor());
          UpdateTomSelect = true;
          _editContext = new EditContext(Flow);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      await base.SetParametersAsync(ParameterView.Empty);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      await base.OnAfterRenderAsync(firstRender);
      if (firstRender)
      {
        ObjectReference = DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("InitDotNet", ObjectReference);
      }
      if (UpdateTomSelect)
      {
        var index = (Flow.FlowDetails.Count - 1).ToString();
        await JSRuntime.InvokeVoidAsync("InitAdvancedSelect", ProgressIdElement + index);
        await JSRuntime.InvokeVoidAsync("InitAdvancedSelect", StartTriggerdElement + index);
        await JSRuntime.InvokeVoidAsync("InitAdvancedSelect", EndTriggerdElement + index);
        UpdateTomSelect = false;
      }
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      ObjectReference?.Dispose();
    }
  }
}