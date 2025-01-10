using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace LGDXRobot2Cloud.UI.Components.Pages.Administration.RobotCertificates;

public sealed partial class RobotCertificateRenew
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

  private RobotCertificateIssueDto? RobotCertificate { get; set; }
  private RobotCertificateRenewViewModel RobotCertificateRenewViewModel { get; set; } = new();
  public readonly List<string> stepHeadings = ["Begin", "Download Cerificates", "Complete"];
  private int currentStep = 0;

  public async Task ReturnLastPage()
  {
    var redirect = await ProtectedSessionStorage.GetAsync<string>("redirectUrl");
    string uri = redirect.Value ?? AppRoutes.Administration.Certificates.Index;
    NavigationManager.NavigateTo(uri);
  }

  public async Task HandleSubmit()
  {
    if (currentStep == 0)
    {
      var response = await RobotCertificateService.RenewRobotCertificateAsync(Id!, Mapper.Map<RobotCertificateRenewRequestDto>(RobotCertificateRenewViewModel));
      if (response.IsSuccess)
      {
        RobotCertificate = response.Data;
        RobotCertificateRenewViewModel.Errors = null;
        currentStep++;
      }
      else
        RobotCertificateRenewViewModel.Errors = response.Errors;
    }
    else if (currentStep == 1)
    {
      RobotCertificate = null;
      currentStep++;
    }
    else 
    {
      await ReturnLastPage();
    }
  }
}
