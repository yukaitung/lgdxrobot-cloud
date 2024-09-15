using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Waypoints;

public sealed partial class WaypointDetail
{
  [Inject]
  public required IWaypointService WaypointService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private Waypoint Waypoint { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  public async Task HandleValidSubmit()
  {
    bool success;

    if (Id != null)
      // Update
      success = await WaypointService.UpdateWaypointAsync((int)Id, Mapper.Map<WaypointUpdateDto>(Waypoint));
    else
      // Create
      success = await WaypointService.AddWaypointAsync(Mapper.Map<WaypointCreateDto>(Waypoint));

    if (success)
      NavigationManager.NavigateTo(AppRoutes.Navigation.Waypoints.Index);
    else 
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var success = await WaypointService.DeleteWaypointAsync((int)Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Waypoints.Index);
      else
        IsError = true;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var waypoint = await WaypointService.GetWaypointAsync((int)_id);
        if (waypoint != null) {
          Waypoint = waypoint;
          _editContext = new EditContext(Waypoint);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        Waypoint = new Waypoint();
        _editContext = new EditContext(Waypoint);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}
