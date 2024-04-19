using AutoMapper;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Waypoints
{
  public partial class WaypointDetail : AbstractForm
  {
    [Inject]
    public required IWaypointService WaypointService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }

    [Parameter]
    public int? Id { get; set; }

    [Parameter]
    public EventCallback<(int, string, CrudOperation)> OnSubmitDone { get; set; }

    private Waypoint Waypoint { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    protected override async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        // Update
        bool success = await WaypointService.UpdateWaypointAsync((int)Id, Mapper.Map<WaypointCreateDto>(Waypoint));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "waypointDetailModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Waypoint.Name, CrudOperation.Update));
        }
        else
          IsError = true;
      }
      else
      {
        // Create
        var success = await WaypointService.AddWaypointAsync(Mapper.Map<WaypointCreateDto>(Waypoint));
        if (success != null)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "waypointDetailModal");
          await OnSubmitDone.InvokeAsync((success.Id, success.Name, CrudOperation.Create));
        }
        else
          IsError = true;
      }
    }

    protected override void HandleInvalidSubmit()
    {
      IsInvalid = true;
    }

    protected override async void HandleDelete()
    {
      if (Id != null)
      {
        var success = await WaypointService.DeleteWaypointAsync((int)Id);
        if (success)
        {
          // DO NOT REVERSE THE ORDER
          await JSRuntime.InvokeVoidAsync("CloseModal", "waypointDeleteModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Waypoint.Name, CrudOperation.Delete));
        } 
        else
          IsError = true;
      }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      parameters.SetParameterProperties(this);
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
      {
        IsInvalid = false;
        IsError = false;
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
}