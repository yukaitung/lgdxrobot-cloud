using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace LGDXRobot2Cloud.UI.Components.Pages.Setting.Certificates;

public sealed partial class CertificateRenew
{
  [Inject]
  public required IRobotCertificateService RobotCertificateService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public ProtectedSessionStorage ProtectedSessionStorage { get; set; } = default!;

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public string? Id { get; set; }

  private RobotCertificateIssueDto? RobotCertificates { get; set; }
  private RobotRenewCertificateRenewDto Settings { get; set; } = new();
  private bool IsError { get; set; } = false;
  public readonly List<string> stepHeadings = ["Begin", "Download Cerificates", "Complete"];
  private int currentStep = 0;

  public async Task ReturnLastPage()
  {
    var redirect = await ProtectedSessionStorage.GetAsync<string>("redirectUrl");
    string uri = redirect.Value ?? AppRoutes.Setting.Certificates.Index;
    NavigationManager.NavigateTo(uri);
  }

  public async Task HandleSubmit()
  {
    if (currentStep == 0)
    {
      var success = await RobotCertificateService.RenewRobotCertificateAsync(Id!, Settings);
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
      await ReturnLastPage();
    }
  }
}
