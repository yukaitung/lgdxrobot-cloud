using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.RobotDetail;

public partial class RenewRobotCertificate
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Parameter]
  public RobotBlazor? Robot { get; set; }

  [Parameter]
  public EventCallback OnUpdated { get; set; }

  private RobotCreateResponseDto? RobotCertificates { get; set; }
  private bool RevokeOldCertificate { get; set; } = false;
  private bool IsError { get; set; } = false;

  private readonly List<string> stepHeadings = ["Begin", "Download Cerificates", "Complete"];
  private int currentStep = 0;

  protected async Task HandleSubmit()
  {
    if (currentStep == 0)
    {
      RobotRenewCertificateRenewDto dto = new RobotRenewCertificateRenewDto {
        RevokeOldCertificate = RevokeOldCertificate
      };
      var success = await RobotService.RenewRobotCertificateAsync(Robot!.Id.ToString(), dto);
      if (success != null)
      {
        RobotCertificates = success;
        RevokeOldCertificate = false;
        IsError = false;
        await OnUpdated.InvokeAsync();
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
      currentStep = 0;
    }
  }
}