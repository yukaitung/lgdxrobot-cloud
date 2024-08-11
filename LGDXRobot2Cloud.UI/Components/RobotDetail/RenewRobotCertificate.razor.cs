using AutoMapper;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.RobotDetail;

public partial class RenewRobotCertificate
{
  private RobotCreateResponseDto? RobotCertificates { get; set; }
  private bool IsError { get; set; } = false;

  private readonly List<string> stepHeadings = ["Begin", "Download Cerificates", "Complete"];
  private int currentStep = 0;

  protected async Task HandleSubmit()
  {
    if (currentStep == 0)
    {
      /*var success = await RobotService.AddRobotAsync(Mapper.Map<RobotCreateDto>(Robot));
      if (success != null)
      {
        RobotCertificates = success;
        IsError = false;
        IsInvalid = false;
        currentStep++;
      }
      else
        IsError = true;*/
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