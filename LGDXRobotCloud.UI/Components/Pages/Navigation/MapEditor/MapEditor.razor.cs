using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;

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

public partial class MapEditor : ComponentBase, IDisposable
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
  private MapEditorViewModel MapEditorViewModel { get; set; } = new()
  {
    IsSuccess = false,
  };
  private WaypointListDto? SelectedWaypoint { get; set; }
  private MapEditorMode MapEditorMode { get; set; } = MapEditorMode.Normal;
  private MapEditorError MapEditorError { get; set; } = MapEditorError.None;
  private int SelectedFromWaypointId { get; set; } = 0;
  private int SelectedToWaypointId { get; set; } = 0;
  bool HasWaypointTrafficControl { get; set; } = false;

  private async Task HandleMapEditorModeChange(MapEditorMode mode)
  {
    if (mode != MapEditorMode.Normal)
    {
      MapEditorError = MapEditorError.None;
    }
    MapEditorMode = mode;
    await JSRuntime.InvokeVoidAsync("MapEditorSetMode", (int)mode);
  }

  private void SaveMapEditorViewModel()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    settings.MapEditorData = MapEditorViewModel;
    TokenService.UpdateSessionSettings(user, settings);
  }

  public void OnEditWaypointClick(int id)
  {
    NavigationManager.NavigateTo($"{AppRoutes.Navigation.Waypoints.Index}/{id}?ReturnUrl={AppRoutes.Navigation.MapEditor.Index}");
  }

  public void HandelResetMapEditor()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    settings.MapEditorData = null;
    TokenService.UpdateSessionSettings(user, settings);
    NavigationManager.Refresh(true);
  }

  public async Task HandelSubmit()
  {
    MapEditorViewModel.ClearMessages();
    try
    {
      var realmId = Realm.Id ?? 0;
      await LgdxApiClient.Navigation.MapEditor[realmId].PostAsync(MapEditorViewModel.ToUpdateDto());
      MapEditorViewModel.IsSuccess = true;
    }
    catch (ApiException ex)
    {
      MapEditorViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
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
    if (MapEditorViewModel.WaypointTraffics.Any(x => x.WaypointFromId == SelectedFromWaypointId && x.WaypointToId == SelectedToWaypointId)
      || MapEditorViewModel.WaypointTraffics.Any(x => x.WaypointFromId == SelectedToWaypointId && x.WaypointToId == SelectedFromWaypointId))
    {
      isValid = false;
      MapEditorError = MapEditorError.HasTraffic;
    }

    if (isValid)
    {
      // Update View Model
      MapEditorViewModel.WaypointTraffics.Add(new WaypointTrafficDto
      {
        WaypointFromId = SelectedFromWaypointId,
        WaypointToId = SelectedToWaypointId,
      });
      if (isBothWaysTraffic)
      {
        MapEditorViewModel.WaypointTraffics.Add(new WaypointTrafficDto
        {
          WaypointFromId = SelectedToWaypointId,
          WaypointToId = SelectedFromWaypointId,
        });
      }
      // Update Map Editor
      var traffic = new WaypointTrafficDisplay
      {
        WaypointFromId = SelectedFromWaypointId,
        WaypointToId = SelectedToWaypointId,
        IsBothWaysTraffic = isBothWaysTraffic,
      };
      MapEditorViewModel.WaypointTrafficsDisplay.Add(traffic);
      List<WaypointTrafficDisplay> t1 = [traffic];
      await JSRuntime.InvokeVoidAsync("MapEditorAddTraffics", t1);
      SaveMapEditorViewModel();
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
    StateHasChanged();
  }

  [JSInvokable("HandleDeleteTraffic")]
  public async Task HandleDeleteTraffic(string waypointIds)
  {
    var ids = waypointIds.Split('-');
    int fromWaypointId = int.Parse(ids[0]);
    int toWaypointId = int.Parse(ids[1]);

    // Delete model
    MapEditorViewModel.WaypointTraffics.RemoveAll(x => x.WaypointFromId == fromWaypointId && x.WaypointToId == toWaypointId);
    MapEditorViewModel.WaypointTraffics.RemoveAll(x => x.WaypointFromId == toWaypointId && x.WaypointToId == fromWaypointId);
    // Delete display
    MapEditorViewModel.WaypointTrafficsDisplay.RemoveAll(x => x.WaypointFromId == fromWaypointId && x.WaypointToId == toWaypointId);
    MapEditorViewModel.WaypointTrafficsDisplay.RemoveAll(x => x.WaypointFromId == toWaypointId && x.WaypointToId == fromWaypointId);

    await HandleMapEditorModeChange(MapEditorMode.Normal);
    SaveMapEditorViewModel();
    StateHasChanged();
  }

  [JSInvokable("HandleWaypointSelect")]
  public void HandleWaypointSelect(string waypointId)
  {
    // Remove prefix w-
    int id = int.Parse(waypointId);
    SelectedWaypoint = MapEditorViewModel.Waypoints.FirstOrDefault(w => w.Id == id);
    StateHasChanged();
  }

  protected override async Task OnInitializedAsync()
  {
    // Get Realm
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    Realm = await CachedRealmService.GetCurrrentRealmAsync(settings.CurrentRealmId);
    RealmName = Realm.Name ?? string.Empty;
    HasWaypointTrafficControl = await CachedRealmService.GetHasWaypointTrafficControlAsync(settings.CurrentRealmId);
    await base.OnInitializedAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitNavigationMap", ObjectReference);

      // Get Map Editor data
      var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
      var settings = TokenService.GetSessionSettings(user);
      var mapEditor = settings.MapEditorData;
      if (mapEditor != null)
      {
        MapEditorViewModel = mapEditor;

        // Update Waypoint
        if (!string.IsNullOrWhiteSpace(UpdateWaypointId))
        {
          var waypoint = await LgdxApiClient.Navigation.Waypoints[int.Parse(UpdateWaypointId)].GetAsync();
          if (waypoint != null)
          {
            var newWaypoint = new WaypointListDto
            {
              Id = waypoint.Id,
              Name = waypoint.Name,
              Realm = new RealmSearchDto
              {
                Id = waypoint.Realm!.Id,
                Name = waypoint.Realm!.Name,
              },
              X = waypoint.X,
              Y = waypoint.Y,
              Rotation = waypoint.Rotation,
            };
            MapEditorViewModel.Waypoints.RemoveAll(w => w.Id == waypoint.Id);
            MapEditorViewModel.Waypoints.Add(newWaypoint);
            SaveMapEditorViewModel();
          }
        }
        // Delete Waypoint
        if (!string.IsNullOrWhiteSpace(DeleteWaypointId))
        {
          MapEditorViewModel.Waypoints.RemoveAll(w => w.Id == int.Parse(DeleteWaypointId));
          SaveMapEditorViewModel();
        }
      }
      else
      {
        var realmId = Realm.Id ?? 0;
        var me = await LgdxApiClient.Navigation.MapEditor[realmId].GetAsync();
        if (me != null)
          MapEditorViewModel.FromDto(me);
      }
      // Update Map Editor
      if (MapEditorViewModel.Waypoints.Count > 0)
      {
        await JSRuntime.InvokeVoidAsync("MapEditorAddWaypoints", MapEditorViewModel.Waypoints);
      }
      if (MapEditorViewModel.WaypointTrafficsDisplay.Count > 0)
      {
        await JSRuntime.InvokeVoidAsync("MapEditorAddTraffics", MapEditorViewModel.WaypointTrafficsDisplay);
      }
    }
    await base.OnAfterRenderAsync(firstRender);
  }

  public void Dispose()
  {
    ObjectReference?.Dispose();
    GC.SuppressFinalize(this);
  }
}