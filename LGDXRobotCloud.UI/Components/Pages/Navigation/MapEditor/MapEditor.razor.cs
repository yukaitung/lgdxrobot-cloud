using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.MapEditor;

public enum MapEditorMode
{
  Normal = 0,
  SingleWayTrafficFrom = 1,
  SingleWayTrafficTo = 2,
  BothWaysTrafficFrom = 3,
  BothWaysTrafficTo = 4,
  DeleteTraffic = 5,
}

public enum MapEditorError
{
  None = 0,
  SameWaypoint = 1,
  HasTraffic = 2,
}

public sealed partial class MapEditor : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [SupplyParameterFromQuery]
  private string? UpdateWaypointId { get; set; }

  [SupplyParameterFromQuery]
  private string? DeleteWaypointId { get; set; }

  private DotNetObjectReference<MapEditor> ObjectReference = null!;
  private string RealmName { get; set; } = string.Empty;
  private RealmDto Realm { get; set; } = null!;
  private MapEditorViewModel MapEditorViewModel { get; set; } = new();
  private WaypointListDto? SelectedWaypoint { get; set; }
  private MapEditorMode MapEditorMode { get; set; } = MapEditorMode.Normal;
  private MapEditorError MapEditorError { get; set; } = MapEditorError.None;
  private int SelectedFromWaypointId { get; set; } = 0;
  private int SelectedToWaypointId { get; set; } = 0;

  public async Task HandleMapEditorModeChange(MapEditorMode mode)
  {
    if (mode != MapEditorMode.Normal)
    {
      MapEditorError = MapEditorError.None;
    }
    MapEditorMode = mode;
    await JSRuntime.InvokeVoidAsync("MapEditorSetMode", (int)mode);
    StateHasChanged();
  }

  public async Task CheckAndAddTraffic(bool isBothWaysTraffic)
  {
    bool isValid = true;
    // The two waypoint must not be the same
    if (SelectedFromWaypointId == SelectedToWaypointId)
    {
      isValid = false;
      MapEditorError = MapEditorError.SameWaypoint;
    }
    // The two waypoint must not have any traffic
    if (MapEditorViewModel.WaypointLinks.Any(x => x.WaypointFromId == SelectedFromWaypointId && x.WaypointToId == SelectedToWaypointId)
      || MapEditorViewModel.WaypointLinks.Any(x => x.WaypointFromId == SelectedToWaypointId && x.WaypointToId == SelectedFromWaypointId))
    {
      isValid = false;
      MapEditorError = MapEditorError.HasTraffic;
    }

    if (isValid)
    {
      // Update View Model
      MapEditorViewModel.WaypointLinks.Add(new WaypointLinkDto
      {
        WaypointFromId = SelectedFromWaypointId,
        WaypointToId = SelectedToWaypointId,
      });
      if (isBothWaysTraffic)
      {
        MapEditorViewModel.WaypointLinks.Add(new WaypointLinkDto
        {
          WaypointFromId = SelectedToWaypointId,
          WaypointToId = SelectedFromWaypointId,
        });
      }
      // Update Map Editor
      var traffic = new WaypointLinkDisplay
      {
        WaypointFromId = SelectedFromWaypointId,
        WaypointToId = SelectedToWaypointId,
        IsBothWaysTraffic = isBothWaysTraffic,
      };
      MapEditorViewModel.WaypointLinksDisplay.Add(traffic);
      List<WaypointLinkDisplay> t1 = [traffic];
      await JSRuntime.InvokeVoidAsync("MapEditorAddLinks", t1);
    }

    await HandleMapEditorModeChange(MapEditorMode.Normal);
  }

  [JSInvokable("HandleAddTraffic")]
  public async Task HandleAddTraffic(string waypointId)
  {
    int id = int.Parse(waypointId);
    switch (MapEditorMode)
    {
      case MapEditorMode.SingleWayTrafficFrom:
        // Save WaypointFromId and ask WaypointToId
        SelectedFromWaypointId = id;
        await HandleMapEditorModeChange(MapEditorMode.SingleWayTrafficTo);
        break;
      case MapEditorMode.SingleWayTrafficTo:
        // Save WaypointToId and update map
        SelectedToWaypointId = id;
        await CheckAndAddTraffic(false);
        break;
      case MapEditorMode.BothWaysTrafficFrom:
        // Save WaypointFromId and ask WaypointToId
        SelectedFromWaypointId = id;
        await HandleMapEditorModeChange(MapEditorMode.BothWaysTrafficTo);
        break;
      case MapEditorMode.BothWaysTrafficTo:
        // Save WaypointToId and update map
        SelectedToWaypointId = id;
        await CheckAndAddTraffic(true);
        break;
    }
  }

  [JSInvokable("HandleDeleteTraffic")]
  public async Task HandleDeleteTraffic(string waypointIds)
  {
    var ids = waypointIds.Split('-');
    int fromWaypointId = int.Parse(ids[0]);
    int toWaypointId = int.Parse(ids[1]);

    // Delete model
    MapEditorViewModel.WaypointLinks.RemoveAll(x => x.WaypointFromId == fromWaypointId && x.WaypointToId == toWaypointId);
    MapEditorViewModel.WaypointLinks.RemoveAll(x => x.WaypointFromId == toWaypointId && x.WaypointToId == fromWaypointId);
    // Delete display
    MapEditorViewModel.WaypointLinksDisplay.RemoveAll(x => x.WaypointFromId == fromWaypointId && x.WaypointToId == toWaypointId);
    MapEditorViewModel.WaypointLinksDisplay.RemoveAll(x => x.WaypointFromId == toWaypointId && x.WaypointToId == fromWaypointId);

    await HandleMapEditorModeChange(MapEditorMode.Normal);
  }

  [JSInvokable("HandleWaypointSelect")]
  public void HandleWaypointSelect(string waypointId)
  {
    // Remove prefix w-
    int id = int.Parse(waypointId);
    SelectedWaypoint = MapEditorViewModel.Waypoints.FirstOrDefault(w => w.Id == id);
    StateHasChanged();
  }

  private void OnEditWaypointClick(int id)
  {
    NavigationManager.NavigateTo($"{AppRoutes.Navigation.Waypoints.Index}/{id}?ReturnUrl={AppRoutes.Navigation.MapEditor.Index}");
  }

  protected override async Task OnInitializedAsync()
  {
    // Get Realm
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    Realm = await CachedRealmService.GetCurrrentRealmAsync(settings.CurrentRealmId);
    RealmName = Realm.Name ?? string.Empty;

    await base.OnInitializedAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitNavigationMap", ObjectReference);

      // Get Map Editor
      var realmId = Realm.Id ?? 0;
      var mapEditor = await LgdxApiClient.Navigation.MapEditor[realmId].GetAsync();
      if (mapEditor != null)
      {
        MapEditorViewModel.FromDto(mapEditor);
        if (MapEditorViewModel.Waypoints.Count > 0)
        {
          await JSRuntime.InvokeVoidAsync("MapEditorAddWaypoints", MapEditorViewModel.Waypoints);
        }
        if (MapEditorViewModel.WaypointLinksDisplay.Count > 0)
        {
          await JSRuntime.InvokeVoidAsync("MapEditorAddLinks", MapEditorViewModel.WaypointLinksDisplay);
        }
      }
    }
    await base.OnAfterRenderAsync(firstRender);
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}