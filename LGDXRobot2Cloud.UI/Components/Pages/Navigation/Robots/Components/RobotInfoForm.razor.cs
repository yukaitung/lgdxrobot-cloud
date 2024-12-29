using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots.Components;

public sealed partial class RobotInfoForm : ComponentBase, IDisposable
{
  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IRealmService RealmService { get; set; }

  [Parameter]
  public RobotDetailViewModel? Robot { get; set; }

  private DotNetObjectReference<RobotInfoForm> ObjectReference = null!;

  // Form
  [JSInvokable("HandlSelectSearch")]
  public async Task HandlSelectSearch(string elementId, string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    var response = await RealmService.SearchRealmsAsync(name);
    await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, response.Data);
  }

  [JSInvokable("HandleSelectChange")]
  public void HandleSelectChange(string elementId, int? id, string? name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    var index = elementId.IndexOf('-');
    if (index == -1 || index + 1 == elementId.Length)
      return;
    Robot!.RealmId = id;
    Robot!.RealmName = name;
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender)
    {
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitDotNet", ObjectReference);
      string[] advanceSelectElements = [$"{nameof(RobotDetailViewModel.RealmId)}-"];
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", advanceSelectElements, 0, 1);
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}