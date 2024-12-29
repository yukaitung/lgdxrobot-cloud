using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Waypoints;

public sealed partial class WaypointDetail : ComponentBase, IDisposable
{
  [Inject]
  public required IWaypointService WaypointService { get; set; }

  [Inject]
  public required IRealmService RealmService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<WaypointDetail> ObjectReference = null!;
  private WaypointDetailViewModel WaypointDetailViewModel { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

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
    WaypointDetailViewModel.RealmId = id;
    WaypointDetailViewModel.RealmName = name;
  }

  public async Task HandleValidSubmit()
  {
    ApiResponse<bool> response;
    if (Id != null)
      // Update
      response = await WaypointService.UpdateWaypointAsync((int)Id, Mapper.Map<WaypointUpdateDto>(WaypointDetailViewModel));
    else
      // Create
      response = await WaypointService.AddWaypointAsync(Mapper.Map<WaypointCreateDto>(WaypointDetailViewModel));
    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Navigation.Waypoints.Index);
    else 
      WaypointDetailViewModel.Errors = response.Errors;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await WaypointService.DeleteWaypointAsync((int)Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Waypoints.Index);
      else
        WaypointDetailViewModel.Errors = response.Errors;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null && WaypointDetailViewModel == null)
      {
        var response = await WaypointService.GetWaypointAsync((int)_id);
        var waypoint = response.Data;
        if (waypoint != null) 
        {
          WaypointDetailViewModel = Mapper.Map<WaypointDetailViewModel>(waypoint);
          _editContext = new EditContext(WaypointDetailViewModel);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        WaypointDetailViewModel = new WaypointDetailViewModel();
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
