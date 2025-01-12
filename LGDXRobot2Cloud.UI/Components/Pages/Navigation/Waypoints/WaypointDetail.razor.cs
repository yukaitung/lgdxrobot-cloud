using System.Text.Json;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Waypoints;

public sealed partial class WaypointDetail : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<WaypointDetail> ObjectReference = null!;
  private WaypointDetailViewModel WaypointDetailViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  // Form
  [JSInvokable("HandlSelectSearch")]
  public async Task HandlSelectSearch(string elementId, string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    var response = await LgdxApiClient.Navigation.Realms.Search.GetAsync(x => x.QueryParameters = new() {
      Name = name
    });
    string result = JsonSerializer.Serialize(response);
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
    WaypointDetailViewModel.RealmId = id != null ? int.Parse(id) : null;
    WaypointDetailViewModel.RealmName = name;
  }

  public async Task HandleValidSubmit()
  {
    if (Id != null)
    {
      // Update
      await LgdxApiClient.Navigation.Waypoints[(int)Id].PutAsync(WaypointDetailViewModel.ToUpdateDto());
    }
    else
    {
      // Create
      await LgdxApiClient.Navigation.Waypoints.PostAsync(WaypointDetailViewModel.ToCreateDto());
    }
    NavigationManager.NavigateTo(AppRoutes.Navigation.Waypoints.Index);
  }

  public async Task HandleDelete()
  {
    await LgdxApiClient.Navigation.Waypoints[(int)Id!].DeleteAsync();
    NavigationManager.NavigateTo(AppRoutes.Navigation.Waypoints.Index);
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var waypoint = await LgdxApiClient.Navigation.Waypoints[(int)_id].GetAsync();
        WaypointDetailViewModel.FromDto(waypoint!);
        _editContext = new EditContext(WaypointDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        _editContext = new EditContext(WaypointDetailViewModel);
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
      string[] advanceSelectElements = [$"{nameof(WaypointDetailViewModel.RealmId)}-"];
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", advanceSelectElements, 0, 1);
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}
