using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Automation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.Progresses;

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

  private ProgressDetailViewModel ProgressDetailViewModel { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleValidSubmit()
  {
    ApiResponse<bool> response;

    if (Id != null)
      // Update
      response = await ProgressService.UpdateProgressAsync((int)Id, Mapper.Map<ProgressUpdateDto>(ProgressDetailViewModel));
    else
      response = await ProgressService.AddProgressAsync(Mapper.Map<ProgressCreateDto>(ProgressDetailViewModel));

    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Automation.Progresses.Index);
    else
      ProgressDetailViewModel.Errors = response.Errors;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await ProgressService.DeleteProgressAsync((int)Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Automation.Progresses.Index);
      else
        ProgressDetailViewModel.Errors = response.Errors;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var response = await ProgressService.GetProgressAsync((int)_id);
        var progress = response.Data;
        if (progress != null) {
          ProgressDetailViewModel = Mapper.Map<ProgressDetailViewModel>(progress);
          _editContext = new EditContext(progress);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        ProgressDetailViewModel = new ProgressDetailViewModel();
        _editContext = new EditContext(ProgressDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}