using LGDXRobotCloud.UI.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.ViewModels.Automation;
using LGDXRobotCloud.UI.Client;
using System.Text.Json;
using Microsoft.Kiota.Abstractions;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.Components.Pages.Automation.Flows;

public partial class FlowDetail : ComponentBase, IDisposable
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<FlowDetail> ObjectReference = null!;
  private FlowDetailViewModel FlowDetailViewModel { get; set; } = new();
  private DeleteEntryModalViewModel DeleteEntryModalViewModel { get; set; } = new();
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
      var response = await LgdxApiClient.Automation.Progresses.Search.GetAsync(x => x.QueryParameters = new() {
        Name = name
      });
      result = JsonSerializer.Serialize(response);
    }
    else if (element == AdvanceSelectElements[1])
    {
      var response = await LgdxApiClient.Automation.Triggers.Search.GetAsync(x => x.QueryParameters = new() {
        Name = name
      });
      result = JsonSerializer.Serialize(response);
    }
    await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, result);
  }

  [JSInvokable("HandleSelectChange")]
  public void HandleSelectChange(string elementId, string? id, string? name)
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
      FlowDetailViewModel.FlowDetails[order].ProgressId = id != null ? int.Parse(id) : null;
      FlowDetailViewModel.FlowDetails[order].ProgressName = name;
    }
    else if (element == AdvanceSelectElements[1])
    {
      FlowDetailViewModel.FlowDetails[order].TriggerId = id != null ? int.Parse(id) : null;
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
    
    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Automation.Flows[FlowDetailViewModel.Id].PutAsync(FlowDetailViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Automation.Flows.PostAsync(FlowDetailViewModel.ToCreateDto());
      }
      NavigationManager.NavigateTo(AppRoutes.Automation.Flows.Index);
    }
    catch (ApiException ex)
    {
      FlowDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleTestDelete()
  {
    DeleteEntryModalViewModel.Errors = null;
    try
    {
      await LgdxApiClient.Automation.Flows[FlowDetailViewModel.Id].TestDelete.PostAsync();
      DeleteEntryModalViewModel.IsReady = true;
    }
    catch (ApiException ex)
    {
      DeleteEntryModalViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Automation.Flows[FlowDetailViewModel.Id].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Automation.Flows.Index);
    }
    catch (ApiException ex)
    {
      FlowDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    } 
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var flow = await LgdxApiClient.Automation.Flows[(int)_id].GetAsync();
        FlowDetailViewModel.FromDto(flow!);
        _editContext = new EditContext(FlowDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
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
    ObjectReference?.Dispose();
    GC.SuppressFinalize(this);
  }
}