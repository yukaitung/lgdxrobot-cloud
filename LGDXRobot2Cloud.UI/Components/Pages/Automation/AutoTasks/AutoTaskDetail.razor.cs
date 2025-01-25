using System.Text.Json;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Automation;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.AutoTasks;

public sealed partial class AutoTaskDetail : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Parameter]
  public int? Id { get; set; }

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  private DotNetObjectReference<AutoTaskDetail> ObjectReference = null!;
  private AutoTaskDetailViewModel AutoTaskDetailViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  // Form helping variables
  private readonly string[] AdvanceSelectElements = [$"{nameof(AutoTaskDetailViewModel.FlowId)}-", $"{nameof(AutoTaskDetailViewModel.AssignedRobotId)}-", $"{nameof(TaskDetailBody.WaypointId)}-"];
  private readonly string[] AdvanceSelectElementsDetail = [$"{nameof(TaskDetailBody.WaypointId)}-"];
  private int InitaisedAdvanceSelect { get; set; } = 0;

  // Form
  public bool IsEditable()
  {
    return Id == null 
      || AutoTaskDetailViewModel.CurrentProgressId == (int)ProgressState.Template;
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
      var response = await LgdxApiClient.Automation.Flows.Search.GetAsync(x => x.QueryParameters = new() {
        Name = name
      });
      result = JsonSerializer.Serialize(response);
    }
    else if (element == AdvanceSelectElements[1])
    {
      var response = await LgdxApiClient.Navigation.Robots.Search.GetAsync(x => x.QueryParameters = new() {
        RealmId = AutoTaskDetailViewModel.RealmId,
        Name = name
      });
      result = JsonSerializer.Serialize(response);
    }
    else if (element == AdvanceSelectElements[2])
    {
      var response = await LgdxApiClient.Navigation.Waypoints.Search.GetAsync(x => x.QueryParameters = new() {
        RealmId = AutoTaskDetailViewModel.RealmId,
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
    if (element == AdvanceSelectElements[0])
    {
      AutoTaskDetailViewModel.FlowId = id != null ? int.Parse(id) : null;
      AutoTaskDetailViewModel.FlowName = name;
    }
    else if (element == AdvanceSelectElements[1])
    {
      AutoTaskDetailViewModel.AssignedRobotId = id != null ? Guid.Parse(id) : null;
      AutoTaskDetailViewModel.AssignedRobotName = name;
    }
    else if (element == AdvanceSelectElements[2])
    {
      AutoTaskDetailViewModel.AutoTaskDetails[order].WaypointId = id != null ? int.Parse(id) : null;
      AutoTaskDetailViewModel.AutoTaskDetails[order].WaypointName = name;
    }
  }

  public void TaskAddStep()
  {
    AutoTaskDetailViewModel.AutoTaskDetails.Add(new TaskDetailBody());
  }

  public async Task TaskStepMoveUp(int i)
  {
    if (i < 1)
      return;
    (AutoTaskDetailViewModel.AutoTaskDetails[i], AutoTaskDetailViewModel.AutoTaskDetails[i - 1]) = (AutoTaskDetailViewModel.AutoTaskDetails[i - 1], AutoTaskDetailViewModel.AutoTaskDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i - 1, i);
  }

  public async Task TaskStepMoveDown(int i)
  {
    if (i > AutoTaskDetailViewModel.AutoTaskDetails.Count - 1)
      return;
    (AutoTaskDetailViewModel.AutoTaskDetails[i], AutoTaskDetailViewModel.AutoTaskDetails[i + 1]) = (AutoTaskDetailViewModel.AutoTaskDetails[i + 1], AutoTaskDetailViewModel.AutoTaskDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1);
  }

  public async Task TaskRemoveStep(int i)
  {
    if (AutoTaskDetailViewModel.AutoTaskDetails.Count <= 0)
      return;
    if (i < AutoTaskDetailViewModel.AutoTaskDetails.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1, true);
    AutoTaskDetailViewModel.AutoTaskDetails.RemoveAt(i);
    InitaisedAdvanceSelect--;
  }

  public async Task HandleValidSubmit()
  {
    // Setup Order
    for (int i = 0; i < AutoTaskDetailViewModel.AutoTaskDetails.Count; i++)
      AutoTaskDetailViewModel.AutoTaskDetails[i].Order = i;

    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Automation.AutoTasks[AutoTaskDetailViewModel.Id].PutAsync(AutoTaskDetailViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Automation.AutoTasks.PostAsync(AutoTaskDetailViewModel.ToCreateDto());
      }
    }
    catch (ApiException ex)
    {
      AutoTaskDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    NavigationManager.NavigateTo(AppRoutes.Automation.AutoTasks.Index);
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Automation.AutoTasks[AutoTaskDetailViewModel.Id].DeleteAsync();
    }
    catch (ApiException ex)
    {
      AutoTaskDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    NavigationManager.NavigateTo(AppRoutes.Automation.AutoTasks.Index);
  }

  public async Task HandleAbort()
  {
    try
    {
      await LgdxApiClient.Automation.AutoTasks[AutoTaskDetailViewModel.Id].Abort.PostAsync();
    }
    catch (ApiException ex)
    {
      AutoTaskDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    NavigationManager.NavigateTo(AppRoutes.Automation.AutoTasks.Index);
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    AutoTaskDetailViewModel.RealmId = settings.CurrentRealmId;
    AutoTaskDetailViewModel.RealmName = CachedRealmService.GetRealmName(settings.CurrentRealmId);
    await base.OnInitializedAsync();
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var task = await LgdxApiClient.Automation.AutoTasks[(int)_id].GetAsync();
        // Normal Assigement
        AutoTaskDetailViewModel.FromDto(task!);
        _editContext = new EditContext(AutoTaskDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        AutoTaskDetailViewModel = new AutoTaskDetailViewModel();
        AutoTaskDetailViewModel.AutoTaskDetails.Add(new TaskDetailBody());
        _editContext = new EditContext(AutoTaskDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }

      var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
      if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("Clone", out var param))
      {
        AutoTaskDetailViewModel.IsClone = bool.Parse(param[0] ?? string.Empty);
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
    if (InitaisedAdvanceSelect < AutoTaskDetailViewModel.AutoTaskDetails.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        AutoTaskDetailViewModel.AutoTaskDetails.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = AutoTaskDetailViewModel.AutoTaskDetails.Count;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}
