using AutoMapper;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace LGDXRobot2Cloud.UI.Components.Pages.Setting.Certificates;

public sealed partial class CertificateDetail
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public ProtectedSessionStorage ProtectedSessionStorage { get; set; } = default!;

  [Inject]
  public required IRobotCertificateService RobotCertificateService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public string? Id { get; set; }

  RobotCertificate? RobotCertificate { get; set; } = null!;

  string RedirectUrl { get; set; } = string.Empty;

  public async Task SetRedirectUrl()
  {
    await ProtectedSessionStorage.SetAsync("redirectUrl", RedirectUrl);
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id) && _id != null)
    {
      var robotCertificate = await RobotCertificateService.GetRobotCertificateAsync(_id);
      if (robotCertificate != null) 
      {
        RobotCertificate = robotCertificate;
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }

  protected override Task OnInitializedAsync()
  {
    RedirectUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
    return base.OnInitializedAsync();
  }
}