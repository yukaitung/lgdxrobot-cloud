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

    private Waypoint _waypoint { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool _isInvalid { get; set; } = false;
    private bool _isError { get; set; } = false;

    protected override async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        // Update
        bool success = await WaypointService.UpdateWaypointAsync((int)Id, Mapper.Map<WaypointCreateDto>(_waypoint));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "waypointDetailModal");
          await OnSubmitDone.InvokeAsync(((int)Id, _waypoint.Name, CrudOperation.Update));
        }
        else
          _isError = true;
      }
      else
      {
        // Create
        var success = await WaypointService.AddWaypointAsync(Mapper.Map<WaypointCreateDto>(_waypoint));
        if (success != null)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "waypointDetailModal");
          await OnSubmitDone.InvokeAsync((success.Id, success.Name, CrudOperation.Create));
        }
        else
          _isError = true;
      }
    }

    protected override void HandleInvalidSubmit()
    {
      _isInvalid = true;
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
          await OnSubmitDone.InvokeAsync(((int)Id, _waypoint.Name, CrudOperation.Delete));
        } 
        else
          _isError = true;
      }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      parameters.SetParameterProperties(this);
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
      {
        _isInvalid = false;
        _isError = false;
        if (_id != null)
        {
          var waypoint = await WaypointService.GetWaypointAsync((int)_id);
          if (waypoint != null) {
            _waypoint = waypoint;
            _editContext = new EditContext(_waypoint);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          }
        }
        else
        {
          _waypoint = new Waypoint();
          _editContext = new EditContext(_waypoint);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      await base.SetParametersAsync(ParameterView.Empty);
    }
  }
}