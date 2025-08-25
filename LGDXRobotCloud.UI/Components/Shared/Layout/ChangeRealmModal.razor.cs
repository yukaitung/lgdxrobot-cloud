using System.Text.Json;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace LGDXRobotCloud.UI.Components.Shared.Layout;

public sealed partial class ChangeRealmModal : ComponentBase, IDisposable
{
  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; }

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }
  
  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  public int RealmId { get; set; } = 0;
  public string? RealmName { get; set; }

  private DotNetObjectReference<ChangeRealmModal> ObjectReference = null!;
  private readonly string SelectId = "ChangeRealm";

  [JSInvokable("HandlSelectSearch")]
  public async Task HandlSelectSearch(string elementId, string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    if (elementId == SelectId)
    {
      var result = await LgdxApiClient.Navigation.Realms.Search.GetAsync(x => x.QueryParameters = new(){
        Name = name
      });
      string str = JsonSerializer.Serialize(result);
      await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", SelectId, str);
    }
  }

  [JSInvokable("HandleSelectChange")]
  public void HandleSelectChange(string elementId, string? id, string? name)
  {
    if (elementId == SelectId)
    {
      RealmId = id != null ? int.Parse(id) : 0;
      RealmName = name;
    }
  }

  public async Task HandleRealmChange()
  {
    await CachedRealmService.GetCurrrentRealmAsync(RealmId); // Preload the realm
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    settings.CurrentRealmId = RealmId;
    TokenService.UpdateSessionSettings(user, settings);
    NavigationManager.Refresh(true);
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    var realm = await CachedRealmService.GetCurrrentRealmAsync(settings.CurrentRealmId);
    RealmId = realm!.Id ?? 0;
    RealmName = realm.Name;
    settings.CurrentRealmId = RealmId;
    TokenService.UpdateSessionSettings(user, settings);
    await base.OnInitializedAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitChangeRealm", ObjectReference);
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelect", SelectId);
    }
    await base.OnAfterRenderAsync(firstRender);
  }
  
  public void Dispose()
  {
    ObjectReference?.Dispose();
    GC.SuppressFinalize(this);
  }
}