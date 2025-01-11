using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Layout;

public sealed partial class ChangeRealmModal : ComponentBase, IDisposable
{
  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; }

  [Inject]
  public required IRealmService RealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  public int? RealmId { get; set; }
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
      var response = await RealmService.SearchRealmsAsync(name);
      if (response.IsSuccess)
      {
        var result = response.Data;
        await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", SelectId, result);
      }
    }
  }

  [JSInvokable("HandleSelectChange")]
  public void HandleSelectChange(string elementId, int? id, string? name)
  {
    if (elementId == SelectId)
    {
      RealmId = id;
      RealmName = name;
    }
  }

  public async Task HandleRealmChange()
  {
    var response = await RealmService.GetCurrrentRealmAsync(RealmId);
    if (response.IsSuccess)
    {
      var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
      var settings = TokenService.GetSessionSettings(user);
      settings.CurrentRealmId = RealmId;
      TokenService.UpdateSessionSettings(user, settings);
      NavigationManager.Refresh(true);
    }
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    var response = await RealmService.GetCurrrentRealmAsync(settings.CurrentRealmId);
    var realm = response.Data;
    RealmId = realm!.Id;
    RealmName = realm.Name;
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
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}