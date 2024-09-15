using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Progresses;

public sealed partial class ProgressDetail : ComponentBase
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IProgressService ProgressService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private Progress Progress { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  public async Task HandleValidSubmit()
  {
    bool success;

    if (Id != null)
      // Update
      success = await ProgressService.UpdateProgressAsync((int)Id, Mapper.Map<ProgressUpdateDto>(Progress));
    else
      success = await ProgressService.AddProgressAsync(Mapper.Map<ProgressCreateDto>(Progress));

    if (success)
      NavigationManager.NavigateTo(AppRoutes.Navigation.Progresses.Index);
    else
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var success = await ProgressService.DeleteProgressAsync((int)Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Progresses.Index);
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
        var progress = await ProgressService.GetProgressAsync((int)_id);
        if (progress != null) {
          Progress = progress;
          _editContext = new EditContext(progress);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        Progress = new Progress();
        _editContext = new EditContext(Progress);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}