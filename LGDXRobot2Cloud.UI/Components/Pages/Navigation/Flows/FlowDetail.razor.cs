using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Helpers;
using Model = LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using LGDXRobot2Cloud.UI.Constants;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Flows;

public sealed partial class FlowDetail : ComponentBase, IDisposable
{
  [Inject]
  public required IFlowService FlowService { get; set; }

  [Inject]
  public required IProgressService ProgressService { get; set; }

  [Inject]
  public required ITriggerService TriggerService { get; set; }

  [Inject]
  public required INodeService NodeService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<FlowDetail> ObjectReference = null!;
  private Model.Flow Flow { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  // Form helping variables
  private readonly string[] AdvanceSelectElements = ["ProgressId-", "TriggerdId-"];
  private int InitaisedAdvanceSelect { get; set; } = 0;

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
    string result = string.Empty;
    if (element == AdvanceSelectElements[0])
    {
      result = await ProgressService.SearchProgressesAsync(name);
    }
    else if (element == AdvanceSelectElements[1])
    {
      result = await TriggerService.SearchTriggersAsync(name);
    }
    else if (element == AdvanceSelectElements[2])
    {
      result = await TriggerService.SearchTriggersAsync(name);
    }
    await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, result);
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
    if (element == AdvanceSelectElements[0] && id != null)
    {
      Flow.FlowDetails[order].ProgressId = id;
      Flow.FlowDetails[order].ProgressName = name;
    }
    else if (element == AdvanceSelectElements[1])
    {
      Flow.FlowDetails[order].TriggerId = id;
      Flow.FlowDetails[order].TriggerName = name;
    }
  }

  public void HandleProceedConditionChange(int i, object? args)
  {
    if (args != null)
      Flow.FlowDetails[i].AutoTaskNextControllerId = int.Parse(args.ToString() ?? string.Empty);
  }

  public void FlowAddStep()
  {
    Flow.FlowDetails.Add(new Model.FlowDetail());
  }

  public async Task FlowStepMoveUp(int i)
  {
    if (i < 1)
      return;
    (Flow.FlowDetails[i], Flow.FlowDetails[i - 1]) = (Flow.FlowDetails[i - 1], Flow.FlowDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i - 1, i);
  }

  public async Task FlowStepMoveDown(int i)
  {
    if (i > Flow.FlowDetails.Count - 1)
      return;
    (Flow.FlowDetails[i], Flow.FlowDetails[i + 1]) = (Flow.FlowDetails[i + 1], Flow.FlowDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1);
  }

  public async Task FlowRemoveStep(int i)
  {
    if (Flow.FlowDetails.Count <= 1)
      return;
    if (i < Flow.FlowDetails.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1, true);
    Flow.FlowDetails.RemoveAt(i);
  }

  public async Task HandleValidSubmit()
  {
    // Setup Order
    for (int i = 0; i < Flow.FlowDetails.Count; i++)
      Flow.FlowDetails[i].Order = i;
    
    bool success;
    if (Id != null)
      // Update
      success = await FlowService.UpdateFlowAsync((int)Id, Mapper.Map<FlowUpdateDto>(Flow));
    else
      // Create
      success = await FlowService.AddFlowAsync(Mapper.Map<FlowCreateDto>(Flow));

    if (success)
      NavigationManager.NavigateTo(AppRoutes.Navigation.Flows.Index);
    else
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var success = await FlowService.DeleteFlowAsync((int)Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Flows.Index);
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
        var flow = await FlowService.GetFlowAsync((int)_id);
        if (flow != null)
        {
          Flow = flow;
          for (int i = 0; i < Flow.FlowDetails.Count; i++)
          {
            Flow.FlowDetails[i].ProgressName = flow.FlowDetails[i].Progress?.Name;
            Flow.FlowDetails[i].ProgressId = flow.FlowDetails[i].Progress?.Id;
            Flow.FlowDetails[i].TriggerId = flow.FlowDetails[i].Trigger?.Id;
            Flow.FlowDetails[i].TriggerName = flow.FlowDetails[i].Trigger?.Name;
          }
          _editContext = new EditContext(Flow);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        Flow = new Model.Flow();
        Flow.FlowDetails.Add(new Model.FlowDetail());
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
    if (InitaisedAdvanceSelect < Flow.FlowDetails.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        Flow.FlowDetails.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = Flow.FlowDetails.Count;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}