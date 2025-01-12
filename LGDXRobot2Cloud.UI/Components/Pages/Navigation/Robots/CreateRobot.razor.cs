using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots;

public sealed partial class CreateRobot
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  private RobotDetailViewModel Robot { get; set; } = new();
  private RobotChassisInfoViewModel RobotChassisInfo { get; set; } = new();
  private RobotCertificateIssueDto? RobotCertificates { get; set; }
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public readonly List<string> stepHeadings = ["Information", "Chassis", "Download Cerificates", "Complete"];
  private int currentStep = 0;

  public async Task HandleValidSubmit()
  {
    if (currentStep == 0)
    {
      _editContext = new EditContext(RobotChassisInfo);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      currentStep++;
    }
    else if (currentStep == 1)
    {
      var response = await LgdxApiClient.Navigation.Robots.PostAsync(Robot.ToCreateDto(RobotChassisInfo.ToCreateDto()));
      Robot.Errors = null;
      RobotCertificates = response;
      currentStep++;
    }
    else if (currentStep == 2)
    {
      RobotCertificates = null;
      currentStep++;
    }
    else 
    {
      NavigationManager.NavigateTo(AppRoutes.Navigation.Robots.Index);
    }
  }

  protected override void OnInitialized()
  {
    _editContext = new EditContext(Robot);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
  }
}
