using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Robots.Components;

public sealed partial class DetailChassisInfoCard
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public RobotDetailViewModel? Robot { get; set; }

  [Parameter]
  public RobotChassisInfoViewModel? RobotChassisInfo { get; set; }

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleValidSubmit()
  {
    await LgdxApiClient.Navigation.Robots[Robot!.Id].Chassis.PutAsync(RobotChassisInfo!.ToUpdateDto());
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