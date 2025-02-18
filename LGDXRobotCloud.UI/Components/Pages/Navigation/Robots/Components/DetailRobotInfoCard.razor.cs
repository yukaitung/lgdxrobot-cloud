using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Robots.Components;

public sealed partial class DetailRobotInfoCard
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public RobotDetailViewModel? Robot { get; set; }

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleValidSubmit()
  {
    await LgdxApiClient.Navigation.Robots[Robot!.Id].PutAsync(Robot!.ToUpdateDto());
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