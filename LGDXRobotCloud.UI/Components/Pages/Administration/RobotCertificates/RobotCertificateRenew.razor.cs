
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.RobotCertificates;

public partial class RobotCertificateRenew
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public string? Id { get; set; }

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  private RobotCertificateIssueDto? RobotCertificate { get; set; }
  private RobotCertificateRenewViewModel RobotCertificateRenewViewModel { get; set; } = new();
  public readonly List<string> stepHeadings = ["Begin", "Download Cerificates", "Complete"];
  private int currentStep = 0;

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
      string uri = ReturnUrl ?? AppRoutes.Administration.RobotCertificates.Index;
      NavigationManager.NavigateTo(uri);
    }
  }
}
