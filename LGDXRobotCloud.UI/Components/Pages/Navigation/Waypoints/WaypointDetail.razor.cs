using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.UI.ViewModels.Navigation;
using LGDXRobotCloud.UI.ViewModels.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Waypoints;

public sealed partial class WaypointDetail : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

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

  private WaypointDetailViewModel WaypointDetailViewModel { get; set; } = new();
  private DeleteEntryModalViewModel DeleteEntryModalViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  private void Redirect(int id)
  {
    if (ReturnUrl != null)
    {
      // Tell Map Editor to update waypoint by given ID
      NavigationManager.NavigateTo($"{ReturnUrl}?UpdateWaypointId={id}");
    }
    else
    {
      NavigationManager.NavigateTo(AppRoutes.Navigation.Waypoints.Index);
    }
  }

  public async Task HandleValidSubmit()
  {
    try
    {
      int id = 0;
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Navigation.Waypoints[(int)Id].PutAsync(WaypointDetailViewModel.ToUpdateDto());
        id = (int)Id;
      }
      else
      {
        // Create
        var response = await LgdxApiClient.Navigation.Waypoints.PostAsync(WaypointDetailViewModel.ToCreateDto());
        id = response?.Id ?? 0;
      }
      Redirect(id);
    }
    catch (ApiException ex)
    {
      WaypointDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleTestDelete()
  {
    DeleteEntryModalViewModel.Errors = null;
    try
    {
      await LgdxApiClient.Navigation.Waypoints[(int)Id!].TestDelete.PostAsync();
      DeleteEntryModalViewModel.IsReady = true;
    }
    catch (ApiException ex)
    {
      DeleteEntryModalViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Navigation.Waypoints[(int)Id!].DeleteAsync();
      Redirect((int)Id!);
    }
    catch (ApiException ex)
    {
      WaypointDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    WaypointDetailViewModel.RealmId = settings.CurrentRealmId;
    WaypointDetailViewModel.RealmName = await CachedRealmService.GetRealmName(settings.CurrentRealmId);
    await base.OnInitializedAsync();
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
}
