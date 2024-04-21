using AutoMapper;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Progresses
{
  public partial class ProgressDetail : AbstractForm
  {
    [Inject]
    public required IProgressService ProgressService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }

    [Parameter]
    public int? Id { get; set; }

    [Parameter]
    public EventCallback<(int, string, CrudOperation)> OnSubmitDone { get; set; }

    private ProgressBlazor Progress { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    protected override async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        // Update
        bool success = await ProgressService.UpdateProgressAsync((int)Id, Mapper.Map<ProgressUpdateDto>(Progress));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "progressDetailModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Progress.Name, CrudOperation.Update));
        }
        else
          IsError = true;
      }
      else
      {
        // Create
        var success = await ProgressService.AddProgressAsync(Mapper.Map<ProgressCreateDto>(Progress));
        if (success != null)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "progressDetailModal");
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
        var success = await ProgressService.DeleteProgressAsync((int)Id);
        if (success)
        {
          // DO NOT REVERSE THE ORDER
          await JSRuntime.InvokeVoidAsync("CloseModal", "progressDeleteModal");
          await OnSubmitDone.InvokeAsync(((int)Id, Progress.Name, CrudOperation.Delete));
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
          var progress = await ProgressService.GetProgressAsync((int)_id);
          if (progress != null) {
            Progress = progress;
            _editContext = new EditContext(progress);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          }
        }
        else
        {
          Progress = new ProgressBlazor();
          _editContext = new EditContext(Progress);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      await base.SetParametersAsync(ParameterView.Empty);
    }
  }
}