using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots.Components;

public sealed partial class DetailChassisInfoCard
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public Models.Robot? Robot { get; set; }

  [Parameter]
  public RobotChassisInfo? RobotChassisInfo { get; set; }

  private bool IsError { get; set; } = false;

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleValidSubmit()
  {
    bool success = await RobotService.UpdateRobotChassisInfoAsync(Robot!.Id.ToString(), Mapper.Map<RobotChassisInfoUpdateDto>(RobotChassisInfo));
    if (!success)
      IsError = true;
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<RobotChassisInfo?>(nameof(RobotChassisInfo), out var _RobotChassisInfo))
    {
      if (_RobotChassisInfo != null)
      {
        _editContext = new EditContext(_RobotChassisInfo);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        RobotChassisInfo = new RobotChassisInfo();
        _editContext = new EditContext(RobotChassisInfo);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}