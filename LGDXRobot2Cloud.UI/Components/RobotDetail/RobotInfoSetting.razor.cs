using AutoMapper;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.RobotDetail;

public partial class RobotInfoSetting
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public RobotBlazor? Robot { get; set; }

  [Parameter]
  public EventCallback OnUpdated { get; set; }

  private bool IsInvalid { get; set; } = false;
  private bool IsError { get; set; } = false;

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

    protected async Task HandleValidSubmit()
  {
    bool success = await RobotService.UpdateRobotInformationAsync(Robot!.Id.ToString(), Mapper.Map<RobotUpdateDto>(Robot));
    if (success)
    {
      await OnUpdated.InvokeAsync();
      IsError = false;
      IsInvalid = false;
    }
    else
      IsError = true;
  }

  protected void HandleInvalidSubmit()
  {
    IsInvalid = true;
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<RobotBlazor?>(nameof(Robot), out var _Robot))
    {
      if (_Robot != null)
      {
        _editContext = new EditContext(_Robot);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        Robot = new RobotBlazor();
        _editContext = new EditContext(Robot);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}