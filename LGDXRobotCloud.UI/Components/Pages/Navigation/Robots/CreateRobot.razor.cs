using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Robots;

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
    Robot.Errors = null;
    if (currentStep == 0)
    {
      _editContext = new EditContext(RobotChassisInfo);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      currentStep++;
    }
    else if (currentStep == 1)
    {
      try
      {
        var response = await LgdxApiClient.Navigation.Robots.PostAsync(Robot.ToCreateDto(RobotChassisInfo.ToCreateDto()));
        RobotCertificates = response;
      }
      catch (ApiException ex)
      {
        Robot.Errors = ApiHelper.GenerateErrorDictionary(ex);
      }   
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
