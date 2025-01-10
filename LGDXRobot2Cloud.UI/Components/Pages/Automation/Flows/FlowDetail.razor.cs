using AutoMapper;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.ViewModels.Automation;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.Flows;

public sealed partial class FlowDetail : ComponentBase, IDisposable
{
  [Inject]
  public required IFlowService FlowService { get; set; }

  [Inject]
  public required IProgressService ProgressService { get; set; }

  [Inject]
  public required ITriggerService TriggerService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<FlowDetail> ObjectReference = null!;
  private FlowDetailViewModel FlowDetailViewModel { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  // Form helping variables
  private readonly string[] AdvanceSelectElements = [$"{nameof(FlowDetailBody.ProgressId)}-", $"{nameof(FlowDetailBody.TriggerId)}-"];
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
      var response = await ProgressService.SearchProgressesAsync(name);
      result = response.Data!;
    }
    else if (element == AdvanceSelectElements[1])
    {
      var response = await TriggerService.SearchTriggersAsync(name);
      result = response.Data!;
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
      FlowDetailViewModel.FlowDetails[order].ProgressId = id;
      FlowDetailViewModel.FlowDetails[order].ProgressName = name;
    }
    else if (element == AdvanceSelectElements[1])
    {
      FlowDetailViewModel.FlowDetails[order].TriggerId = id;
      FlowDetailViewModel.FlowDetails[order].TriggerName = name;
    }
  }

  public void HandleProceedConditionChange(int i, object? args)
  {
    if (args != null)
      FlowDetailViewModel.FlowDetails[i].AutoTaskNextControllerId = int.Parse(args.ToString() ?? string.Empty);
  }

  public void FlowAddStep()
  {
    FlowDetailViewModel.FlowDetails.Add(new FlowDetailBody());
  }

  public async Task FlowStepMoveUp(int i)
  {
    if (i < 1)
      return;
    (FlowDetailViewModel.FlowDetails[i], FlowDetailViewModel.FlowDetails[i - 1]) = (FlowDetailViewModel.FlowDetails[i - 1], FlowDetailViewModel.FlowDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i - 1, i);
  }

  public async Task FlowStepMoveDown(int i)
  {
    if (i > FlowDetailViewModel.FlowDetails.Count - 1)
      return;
    (FlowDetailViewModel.FlowDetails[i], FlowDetailViewModel.FlowDetails[i + 1]) = (FlowDetailViewModel.FlowDetails[i + 1], FlowDetailViewModel.FlowDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1);
  }

  public async Task FlowRemoveStep(int i)
  {
    if (FlowDetailViewModel.FlowDetails.Count <= 1)
      return;
    if (i < FlowDetailViewModel.FlowDetails.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1, true);
    FlowDetailViewModel.FlowDetails.RemoveAt(i);
    InitaisedAdvanceSelect--;
  }

  public async Task HandleValidSubmit()
  {
    // Setup Order
    for (int i = 0; i < FlowDetailViewModel.FlowDetails.Count; i++)
      FlowDetailViewModel.FlowDetails[i].Order = i;
    
    ApiResponse<bool> response;
    if (Id != null)
      // Update
      response = await FlowService.UpdateFlowAsync((int)Id, Mapper.Map<FlowUpdateDto>(FlowDetailViewModel));
    else
      // Create
      response = await FlowService.AddFlowAsync(Mapper.Map<FlowCreateDto>(FlowDetailViewModel));

    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Automation.Flows.Index);
    else
      response.Errors = FlowDetailViewModel.Errors;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await FlowService.DeleteFlowAsync((int)Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Automation.Flows.Index);
      else
        response.Errors = FlowDetailViewModel.Errors;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var response = await FlowService.GetFlowAsync((int)_id);
        var flow = response.Data;
        if (flow != null)
        {
          FlowDetailViewModel = Mapper.Map<FlowDetailViewModel>(flow);
          _editContext = new EditContext(FlowDetailViewModel);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        FlowDetailViewModel = new FlowDetailViewModel();
        FlowDetailViewModel.FlowDetails.Add(new FlowDetailBody());
        _editContext = new EditContext(FlowDetailViewModel);
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
    if (InitaisedAdvanceSelect < FlowDetailViewModel.FlowDetails.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        FlowDetailViewModel.FlowDetails.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = FlowDetailViewModel.FlowDetails.Count;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}