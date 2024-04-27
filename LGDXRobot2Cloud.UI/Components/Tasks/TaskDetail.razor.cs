using AutoMapper;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Utilities;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Tasks
{
  public partial class TaskDetail : AbstractForm, IDisposable
  {
    [Inject]
    public required IAutoTaskService AutoTaskService { get; set; }

    [Inject]
    public required IFlowService FlowService { get; set; }

    [Inject]
    public required IWaypointService WaypointService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }

    [Parameter]
    public int? Id { get; set; }

    [Parameter]
    public int? CloneId { get; set; }

    [Parameter]
    public EventCallback<(int, string?, CrudOperation)> OnSubmitDone { get; set; }

    private DotNetObjectReference<TaskDetail> ObjectReference = null!;
    private AutoTaskBlazor Task { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    // Form helping variables
    private readonly string[] AdvanceSelectElements = ["FlowId-", "WaypointsId-"];
    private readonly string[] AdvanceSelectElementsDetail = ["WaypointsId-"];
    private bool UpdateAdvanceSelect { get; set; } = false;
    private bool UpdateAdvanceSelectList { get; set; } = false;

    // Form
    private bool IsEditable()
    {
      return Id == null 
        || Task.CurrentProgress.Id == (int)ProgressState.Waiting 
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
      if (element == AdvanceSelectElements[0])
      {
        var result = await FlowService.SearchFlowsAsync(name);
        await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, result);
      }
      if (element == AdvanceSelectElements[1])
      {
        var result = await WaypointService.SearchWaypointsAsync(name);
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
      if (element == AdvanceSelectElements[0])
      {
        Task.FlowId = id;
        Task.FlowName = name;
      }
      else if (element == AdvanceSelectElements[1])
      {
        Task.Details[order].WaypointId = id;
        Task.Details[order].WaypointName = name;
      }
    }

    private void TaskAddStep()
    {
      Task.Details.Add(new AutoTaskDetailBlazor());
      UpdateAdvanceSelect = true;
    }

    private async Task TaskStepMoveUp(int i)
    {
      if (i < 1)
        return;
      (Task.Details[i], Task.Details[i - 1]) = (Task.Details[i - 1], Task.Details[i]);
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i - 1, i);
    }

    private async Task TaskStepMoveDown(int i)
    {
      if (i > Task.Details.Count - 1)
        return;
      (Task.Details[i], Task.Details[i + 1]) = (Task.Details[i + 1], Task.Details[i]);
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1);
    }

    private async Task TaskRemoveStep(int i)
    {
      if (Task.Details.Count <= 1)
        return;
      if (i < Task.Details.Count - 1)
        await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1, true);
      Task.Details.RemoveAt(i);
    }

    protected override async Task HandleValidSubmit()
    {
      // Setup Order
      for (int i = 0; i < Task.Details.Count; i++)
      {
        Task.Details[i].Order = i;
      }
      if (Id != null)
      {
        // Update
        bool success = await AutoTaskService.UpdateAutoTaskAsync((int)Id, Mapper.Map<AutoTaskUpdateDto>(Task));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "taskDetailModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Task.Name, CrudOperation.Update));
        }
        else
          IsError = true;
      }
      else
      {
        // Create
        var success = await AutoTaskService.AddAutoTaskAsync(Mapper.Map<AutoTaskCreateDto>(Task));
        if (success != null)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "taskDetailModal");
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
        var success = await AutoTaskService.DeleteAutoTaskAsync((int)Id);
        if (success)
        {
          // DO NOT REVERSE THE ORDER
          await JSRuntime.InvokeVoidAsync("CloseModal", "taskDeleteModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Task.Name, CrudOperation.Delete));
        }
        else
          IsError = true;
      }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      parameters.SetParameterProperties(this);
      
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id) && parameters.TryGetValue<int?>(nameof(CloneId), out var _cloneId) )
      {
        IsInvalid = false;
        IsError = false;
        if (_id != null)
        {
          var task = await AutoTaskService.GetAutoTaskAsync((int)_id);
          if (task != null)
          {
            Task = task;
            Task.FlowId = Task.Flow.Id;
            Task.FlowName = Task.Flow.Name;
            for (int i = 0; i < Task.Details.Count; i++)
            {
              Task.Details[i].WaypointName = Task.Details[i].Waypoint?.Name;
              Task.Details[i].WaypointId = Task.Details[i].Waypoint?.Id;
            }
            _editContext = new EditContext(Task);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
            UpdateAdvanceSelectList = true;
          }
        }
        else if (_cloneId != null)
        {
          var task = await AutoTaskService.GetAutoTaskAsync((int)_cloneId);
          if (task != null)
          {
            Task = task;
            Task.FlowId = Task.Flow.Id;
            Task.FlowName = Task.Flow.Name;
            for (int i = 0; i < Task.Details.Count; i++)
            {
              Task.Details[i].WaypointName = Task.Details[i].Waypoint?.Name;
              Task.Details[i].WaypointId = Task.Details[i].Waypoint?.Id;
            }
            _editContext = new EditContext(Task);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
            UpdateAdvanceSelectList = true;
          }
        }
        else
        {
          Task = new AutoTaskBlazor();
          Task.Details.Add(new AutoTaskDetailBlazor());
          _editContext = new EditContext(Task);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          UpdateAdvanceSelect = true;
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
      if (UpdateAdvanceSelect)
      {
        var index = (Task.Details.Count - 1).ToString();
        await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", AdvanceSelectElements, index, 1);
        UpdateAdvanceSelect = false;
      }
      else if (UpdateAdvanceSelectList)
      {
        var index = Task.Details.Count.ToString();
        await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", AdvanceSelectElements, 0, index);
        UpdateAdvanceSelectList = false;
      }
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      ObjectReference?.Dispose();
    }
  }
}