using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots.Components;

public sealed partial class DetailRobotInfoCard
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public Models.Robot? Robot { get; set; }

  private bool IsError { get; set; } = false;

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleValidSubmit()
  {
    bool success = await RobotService.UpdateRobotInformationAsync(Robot!.Id.ToString(), Mapper.Map<RobotUpdateDto>(Robot));
    if (!success)
      IsError = true;
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<Models.Robot?>(nameof(Robot), out var _Robot))
    {
      if (_Robot != null)
      {
        _editContext = new EditContext(_Robot);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        Robot = new Models.Robot();
        _editContext = new EditContext(Robot);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}