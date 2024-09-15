using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
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
  private AutoTask Task { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;
  private bool IsClone { get; set; } = false;

  // Form helping variables
  private readonly string[] AdvanceSelectElements = ["FlowId-", "AssignedRobotId-", "WaypointsId-"];
  private readonly string[] AdvanceSelectElementsDetail = ["WaypointsId-"];
  private int InitaisedAdvanceSelect { get; set; } = 0;

  // Form
  public bool IsEditable()
  {
    return Id == null 
      || Task.CurrentProgress.Id == (int)ProgressState.Template;
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
      result = await FlowService.SearchFlowsAsync(name);
    }
    else if (element == AdvanceSelectElements[1])
    {
      result = await RobotService.SearchRobotsAsync(name);
    }
    else if (element == AdvanceSelectElements[2])
    {
      result = await WaypointService.SearchWaypointsAsync(name);
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
      Task.FlowId = id != null ? int.Parse(id) : null;
      Task.FlowName = name;
    }
    else if (element == AdvanceSelectElements[1])
    {
      Task.AssignedRobotId = id != null ? Guid.Parse(id) : null;
      Task.AssignedRobotName = name;
    }
    else if (element == AdvanceSelectElements[2])
    {
      Task.Details[order].WaypointId = id != null ? int.Parse(id) : null;
      Task.Details[order].WaypointName = name;
    }
  }

  public void TaskAddStep()
  {
    Task.Details.Add(new AutoTaskDetail());
  }

  public async Task TaskStepMoveUp(int i)
  {
    if (i < 1)
      return;
    (Task.Details[i], Task.Details[i - 1]) = (Task.Details[i - 1], Task.Details[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i - 1, i);
  }

  public async Task TaskStepMoveDown(int i)
  {
    if (i > Task.Details.Count - 1)
      return;
    (Task.Details[i], Task.Details[i + 1]) = (Task.Details[i + 1], Task.Details[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1);
  }

  public async Task TaskRemoveStep(int i)
  {
    if (Task.Details.Count <= 1)
      return;
    if (i < Task.Details.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1, true);
    Task.Details.RemoveAt(i);
  }

  public async Task HandleValidSubmit()
  {
    // Setup Order
    for (int i = 0; i < Task.Details.Count; i++)
      Task.Details[i].Order = i;

    bool success;
    if (Id != null)
      // Update
      success = await AutoTaskService.UpdateAutoTaskAsync((int)Id, Mapper.Map<AutoTaskUpdateDto>(Task));
    else
      // Create
      success = await AutoTaskService.AddAutoTaskAsync(Mapper.Map<AutoTaskCreateDto>(Task));

    if (success)
      NavigationManager.NavigateTo(AppRoutes.Navigation.Tasks.Index);
    else
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var success = await AutoTaskService.DeleteAutoTaskAsync((int)Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Tasks.Index);
      else
        IsError = true;
    }
  }

  public async Task HandleAbort()
  {
    if (Id != null)
    {
      var success = await AutoTaskService.AbortAutoTaskAsync((int)Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Tasks.Index);
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
        var task = await AutoTaskService.GetAutoTaskAsync((int)_id);
        if (task != null)
        {
          // Normal Assigement
          Task = task;
          Task.FlowId = Task.Flow.Id;
          Task.FlowName = Task.Flow.Name;
          Task.AssignedRobotId = Task.AssignedRobot?.Id;
          Task.AssignedRobotName = Task.AssignedRobot?.Name;
          for (int i = 0; i < Task.Details.Count; i++)
          {
            Task.Details[i].WaypointName = Task.Details[i].Waypoint?.Name;
            Task.Details[i].WaypointId = Task.Details[i].Waypoint?.Id;
          }
          _editContext = new EditContext(Task);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        Task = new AutoTask();
        Task.Details.Add(new AutoTaskDetail());
        _editContext = new EditContext(Task);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }

      var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
      if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("Clone", out var param))
      {
        IsClone = bool.Parse(param[0] ?? string.Empty);
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
    if (InitaisedAdvanceSelect < Task.Details.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        Task.Details.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = Task.Details.Count;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}
