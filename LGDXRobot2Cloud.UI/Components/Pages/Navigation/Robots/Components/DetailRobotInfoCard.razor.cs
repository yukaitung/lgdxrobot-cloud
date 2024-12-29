using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots.Components;

public sealed partial class DetailRobotInfoCard
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public RobotDetailViewModel? Robot { get; set; }

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleValidSubmit()
  {
    var response = await RobotService.UpdateRobotAsync(Robot!.Id.ToString(), Mapper.Map<RobotUpdateDto>(Robot));
    if (!response.IsSuccess)
      Robot.Errors = response.Errors;
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<RobotDetailViewModel?>(nameof(Robot), out var _Robot))
    {
      if (_Robot != null)
      {
        _editContext = new EditContext(_Robot);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        Robot = new RobotDetailViewModel();
        _editContext = new EditContext(Robot);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}