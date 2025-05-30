using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.MapEditor;

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
  private MapEditorDto? MapEditorData { get; set; }
  private WaypointListDto? SelectedWaypoint { get; set; }

  [JSInvokable("HandleWaypointSelect")]
  public void HandleWaypointSelect(string waypointId)
  {
    // Remove prefix w-
    int id = int.Parse(waypointId[2..]);
    SelectedWaypoint = MapEditorData?.Waypoints?.FirstOrDefault(w => w.Id == id);
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
      MapEditorData = await LgdxApiClient.Navigation.MapEditor[realmId].GetAsync();
      if (MapEditorData?.Waypoints?.Count > 0)
      {
        await JSRuntime.InvokeVoidAsync("MapEditorAddWaypoints", MapEditorData.Waypoints);
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