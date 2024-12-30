using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Automation;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Tasks;

public sealed partial class TaskDetail : ComponentBase, IDisposable
{
  [Inject]
  public required IAutoTaskService AutoTaskService { get; set; }

  [Inject]
  public required IFlowService FlowService { get; set; }

  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IWaypointService WaypointService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<TaskDetail> ObjectReference = null!;
  private TaskDetailViewModel TaskDetailViewModel { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  // Form helping variables
  private readonly string[] AdvanceSelectElements = ["FlowId-", "AssignedRobotId-", "WaypointsId-"];
  private readonly string[] AdvanceSelectElementsDetail = ["WaypointsId-"];
  private int InitaisedAdvanceSelect { get; set; } = 0;

  // Form
  public bool IsEditable()
  {
    return Id == null 
      || TaskDetailViewModel.CurrentProgressId == (int)ProgressState.Template;
  }

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
      var response = await FlowService.SearchFlowsAsync(name);
      result = response.Data!;
    }
    else if (element == AdvanceSelectElements[1])
    {
      var response = await RobotService.SearchRobotsAsync(name);
      result = response.Data!;
    }
    else if (element == AdvanceSelectElements[2])
    {
      var response = await WaypointService.SearchWaypointsAsync(name);
      result = response.Data!;
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
    if (element == AdvanceSelectElements[0])
    {
      TaskDetailViewModel.FlowId = id != null ? int.Parse(id) : null;
      TaskDetailViewModel.FlowName = name;
    }
    else if (element == AdvanceSelectElements[1])
    {
      TaskDetailViewModel.AssignedRobotId = id != null ? Guid.Parse(id) : null;
      TaskDetailViewModel.AssignedRobotName = name;
    }
    else if (element == AdvanceSelectElements[2])
    {
      TaskDetailViewModel.AutoTaskDetails[order].WaypointId = id != null ? int.Parse(id) : null;
      TaskDetailViewModel.AutoTaskDetails[order].WaypointName = name;
    }
  }

  public void TaskAddStep()
  {
    TaskDetailViewModel.AutoTaskDetails.Add(new TaskDetailBody());
  }

  public async Task TaskStepMoveUp(int i)
  {
    if (i < 1)
      return;
    (TaskDetailViewModel.AutoTaskDetails[i], TaskDetailViewModel.AutoTaskDetails[i - 1]) = (TaskDetailViewModel.AutoTaskDetails[i - 1], TaskDetailViewModel.AutoTaskDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i - 1, i);
  }

  public async Task TaskStepMoveDown(int i)
  {
    if (i > TaskDetailViewModel.AutoTaskDetails.Count - 1)
      return;
    (TaskDetailViewModel.AutoTaskDetails[i], TaskDetailViewModel.AutoTaskDetails[i + 1]) = (TaskDetailViewModel.AutoTaskDetails[i + 1], TaskDetailViewModel.AutoTaskDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1);
  }

  public async Task TaskRemoveStep(int i)
  {
    if (TaskDetailViewModel.AutoTaskDetails.Count <= 1)
      return;
    if (i < TaskDetailViewModel.AutoTaskDetails.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1, true);
    TaskDetailViewModel.AutoTaskDetails.RemoveAt(i);
    InitaisedAdvanceSelect--;
  }

  public async Task HandleValidSubmit()
  {
    // Setup Order
    for (int i = 0; i < TaskDetailViewModel.AutoTaskDetails.Count; i++)
      TaskDetailViewModel.AutoTaskDetails[i].Order = i;

    ApiResponse<bool> response;
    if (Id != null)
      // Update
      response = await AutoTaskService.UpdateAutoTaskAsync((int)Id, Mapper.Map<AutoTaskUpdateDto>(TaskDetailViewModel));
    else
      // Create
      response = await AutoTaskService.AddAutoTaskAsync(Mapper.Map<AutoTaskCreateDto>(TaskDetailViewModel));

    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Navigation.Tasks.Index);
    else
      TaskDetailViewModel.Errors = response.Errors;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await AutoTaskService.DeleteAutoTaskAsync((int)Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Tasks.Index);
      else
        TaskDetailViewModel.Errors = response.Errors;
    }
  }

  public async Task HandleAbort()
  {
    if (Id != null)
    {
      var response = await AutoTaskService.AbortAutoTaskAsync((int)Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Tasks.Index);
      else
        TaskDetailViewModel.Errors = response.Errors;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var response = await AutoTaskService.GetAutoTaskAsync((int)_id);
        var task = response.Data;
        if (task != null)
        {
          // Normal Assigement
          TaskDetailViewModel = Mapper.Map<TaskDetailViewModel>(task);
          _editContext = new EditContext(TaskDetailViewModel);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        TaskDetailViewModel = new TaskDetailViewModel();
        TaskDetailViewModel.AutoTaskDetails.Add(new TaskDetailBody());
        _editContext = new EditContext(TaskDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }

      var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
      if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("Clone", out var param))
      {
        TaskDetailViewModel.IsClone = bool.Parse(param[0] ?? string.Empty);
        Id = null;
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
    if (InitaisedAdvanceSelect < TaskDetailViewModel.AutoTaskDetails.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        TaskDetailViewModel.AutoTaskDetails.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = TaskDetailViewModel.AutoTaskDetails.Count;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}
