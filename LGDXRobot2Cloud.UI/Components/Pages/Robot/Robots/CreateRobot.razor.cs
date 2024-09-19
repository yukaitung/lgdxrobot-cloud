using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using Model = LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using LGDXRobot2Cloud.UI.Constants;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots;

public sealed partial class CreateRobot
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IMapper Mapper { get; set; }

  private Model.Robot Robot { get; set; } = null!;
  private RobotCertificateIssueDto? RobotCertificates { get; set; }
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  public readonly List<string> stepHeadings = ["Information", "Download Cerificates", "Complete"];
  private int currentStep = 0;

  public async Task HandleValidSubmit()
  {
    if (currentStep == 0)
    {
      var success = await RobotService.AddRobotAsync(Mapper.Map<RobotCreateDto>(Robot));
      if (success != null)
      {
        RobotCertificates = success;
        IsError = false;
        currentStep++;
      }
      else
        IsError = true;
    }
    else if (currentStep == 1)
    {
      RobotCertificates = null;
      currentStep++;
    }
    else 
    {
      NavigationManager.NavigateTo(AppRoutes.Robot.Robots.Index);
    }
  }

  protected override void OnInitialized()
  {
    Robot = new Model.Robot();
    _editContext = new EditContext(Robot);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
  }
}
