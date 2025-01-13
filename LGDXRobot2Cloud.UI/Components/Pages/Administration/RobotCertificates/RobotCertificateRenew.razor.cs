
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobot2Cloud.UI.Components.Pages.Administration.RobotCertificates;

public sealed partial class RobotCertificateRenew
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public ProtectedSessionStorage ProtectedSessionStorage { get; set; } = default!;

  [Parameter]
  public string? Id { get; set; }

  private RobotCertificateIssueDto? RobotCertificate { get; set; }
  private RobotCertificateRenewViewModel RobotCertificateRenewViewModel { get; set; } = new();
  public readonly List<string> stepHeadings = ["Begin", "Download Cerificates", "Complete"];
  private int currentStep = 0;

  public async Task ReturnLastPage()
  {
    var redirect = await ProtectedSessionStorage.GetAsync<string>("redirectUrl");
    string uri = redirect.Value ?? AppRoutes.Administration.RobotCertificates.Index;
    NavigationManager.NavigateTo(uri);
  }

  public async Task HandleSubmit()
  {
    RobotCertificateRenewViewModel.Errors = null;
    if (currentStep == 0)
    {
      if (Guid.TryParse(Id, out Guid _guid))
      {
        try
        {
          RobotCertificate = await LgdxApiClient.Administration.RobotCertificates[_guid].Renew.PostAsync(RobotCertificateRenewViewModel.ToDto());
        }
        catch (ApiException ex)
        {
          RobotCertificateRenewViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
        }
        currentStep++;
      }
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
