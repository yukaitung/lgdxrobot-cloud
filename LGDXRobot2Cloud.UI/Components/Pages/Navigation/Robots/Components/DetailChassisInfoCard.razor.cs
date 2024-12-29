using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots.Components;

public sealed partial class DetailChassisInfoCard
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public RobotDetailViewModel? Robot { get; set; }

  [Parameter]
  public RobotChassisInfoViewModel? RobotChassisInfo { get; set; }

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleValidSubmit()
  {
    var response = await RobotService.UpdateRobotChassisInfoAsync(Robot!.Id.ToString(), Mapper.Map<RobotChassisInfoUpdateDto>(RobotChassisInfo));
    if (!response.IsSuccess)
      RobotChassisInfo!.Errors = response.Errors;
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<RobotChassisInfoViewModel?>(nameof(RobotChassisInfo), out var _RobotChassisInfo))
    {
      if (_RobotChassisInfo != null)
      {
        _editContext = new EditContext(_RobotChassisInfo);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        RobotChassisInfo = new RobotChassisInfoViewModel();
        _editContext = new EditContext(RobotChassisInfo);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}