using System.Text.Json;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.UI.ViewModels.Automation;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Automation.AutoTasks;

public partial class AutoTaskDetails : ComponentBase, IDisposable
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

  private DotNetObjectReference<AutoTaskDetails> ObjectReference = null!;
  private AutoTaskDetailsViewModel AutoTaskDetailsViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  bool HasWaypointTrafficControl { get; set; } = false;
  TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

  // Form helping variables
  private readonly string[] AdvanceSelectElements = [$"{nameof(AutoTaskDetailsViewModel.FlowId)}-", $"{nameof(AutoTaskDetailsViewModel.AssignedRobotId)}-", $"{nameof(TaskDetailBody.WaypointId)}-"];
  private readonly string[] AdvanceSelectElementsDetail = [$"{nameof(TaskDetailBody.WaypointId)}-"];
  private int InitaisedAdvanceSelect { get; set; } = 0;

  // Form
  public bool IsEditable()
  {
    return Id == null 
      || AutoTaskDetailsViewModel.CurrentProgressId == (int)ProgressState.Template;
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
        RealmId = AutoTaskDetailsViewModel.RealmId,
        Name = name
      });
      result = JsonSerializer.Serialize(response);
    }
    else if (element == AdvanceSelectElements[2])
    {
      var response = await LgdxApiClient.Navigation.Waypoints.Search.GetAsync(x => x.QueryParameters = new() {
        RealmId = AutoTaskDetailsViewModel.RealmId,
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
      AutoTaskDetailsViewModel.FlowId = id != null ? int.Parse(id) : null;
      AutoTaskDetailsViewModel.FlowName = name;
    }
    else if (element == AdvanceSelectElements[1])
    {
      AutoTaskDetailsViewModel.AssignedRobotId = id != null ? Guid.Parse(id) : null;
      AutoTaskDetailsViewModel.AssignedRobotName = name;
    }
    else if (element == AdvanceSelectElements[2])
    {
      AutoTaskDetailsViewModel.AutoTaskDetails[order].WaypointId = id != null ? int.Parse(id) : null;
      AutoTaskDetailsViewModel.AutoTaskDetails[order].WaypointName = name;
    }
  }

  public void TaskAddStep()
  {
    AutoTaskDetailsViewModel.AutoTaskDetails.Add(new TaskDetailBody());
  }

  public async Task TaskStepMoveUp(int i)
  {
    if (i < 1)
      return;
    (AutoTaskDetailsViewModel.AutoTaskDetails[i], AutoTaskDetailsViewModel.AutoTaskDetails[i - 1]) = (AutoTaskDetailsViewModel.AutoTaskDetails[i - 1], AutoTaskDetailsViewModel.AutoTaskDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i - 1, i);
  }

  public async Task TaskStepMoveDown(int i)
  {
    if (i > AutoTaskDetailsViewModel.AutoTaskDetails.Count - 1)
      return;
    (AutoTaskDetailsViewModel.AutoTaskDetails[i], AutoTaskDetailsViewModel.AutoTaskDetails[i + 1]) = (AutoTaskDetailsViewModel.AutoTaskDetails[i + 1], AutoTaskDetailsViewModel.AutoTaskDetails[i]);
    await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1);
  }

  public async Task TaskRemoveStep(int i)
  {
    if (AutoTaskDetailsViewModel.AutoTaskDetails.Count <= 0)
      return;
    if (i < AutoTaskDetailsViewModel.AutoTaskDetails.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElementsDetail, i, i + 1, true);
    AutoTaskDetailsViewModel.AutoTaskDetails.RemoveAt(i);
    InitaisedAdvanceSelect--;
  }

  private void Redirect()
  {
    if (ReturnUrl != null)
    {
      NavigationManager.NavigateTo(ReturnUrl);
    }
    else
    {
      NavigationManager.NavigateTo(AppRoutes.Automation.AutoTasks.Index);
    }
  }

  public async Task HandleValidSubmit()
  {
    // Setup Order
    for (int i = 0; i < AutoTaskDetailsViewModel.AutoTaskDetails.Count; i++)
      AutoTaskDetailsViewModel.AutoTaskDetails[i].Order = i;

    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Automation.AutoTasks[AutoTaskDetailsViewModel.Id].PutAsync(AutoTaskDetailsViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Automation.AutoTasks.PostAsync(AutoTaskDetailsViewModel.ToCreateDto());
      }
      Redirect();
    }
    catch (ApiException ex)
    {
      AutoTaskDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Automation.AutoTasks[AutoTaskDetailsViewModel.Id].DeleteAsync();
      Redirect();
    }
    catch (ApiException ex)
    {
      AutoTaskDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
    
  }

  public async Task HandleAbort()
  {
    try
    {
      await LgdxApiClient.Automation.AutoTasks[AutoTaskDetailsViewModel.Id].Abort.PostAsync();
      Redirect();
    }
    catch (ApiException ex)
    {
      AutoTaskDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    TimeZone = settings.TimeZone;
    AutoTaskDetailsViewModel.RealmId = settings.CurrentRealmId;
    AutoTaskDetailsViewModel.RealmName = await CachedRealmService.GetRealmName(settings.CurrentRealmId);
    HasWaypointTrafficControl = await CachedRealmService.GetHasWaypointTrafficControlAsync(settings.CurrentRealmId);
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
        // Normal Assignment
        AutoTaskDetailsViewModel.FromDto(task!);
        _editContext = new EditContext(AutoTaskDetailsViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        AutoTaskDetailsViewModel = new AutoTaskDetailsViewModel();
        AutoTaskDetailsViewModel.AutoTaskDetails.Add(new TaskDetailBody());
        _editContext = new EditContext(AutoTaskDetailsViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }

      var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
      if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("Clone", out var param))
      {
        AutoTaskDetailsViewModel.IsClone = bool.Parse(param[0] ?? string.Empty);
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
    if (InitaisedAdvanceSelect < AutoTaskDetailsViewModel.AutoTaskDetails.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        AutoTaskDetailsViewModel.AutoTaskDetails.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = AutoTaskDetailsViewModel.AutoTaskDetails.Count;
    }
  }

  public void Dispose()
  {
    ObjectReference?.Dispose();
    GC.SuppressFinalize(this);
  }
}
