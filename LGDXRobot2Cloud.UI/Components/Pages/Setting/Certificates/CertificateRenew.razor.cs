using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using Microsoft.AspNetCore.WebUtilities;

namespace LGDXRobot2Cloud.UI.Components.Pages.Setting.Certificates;

public sealed partial class CertificateRenew
{
  [Inject]
  public required IRobotCertificateService RobotCertificateService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public string? Id { get; set; }

  private RobotCertificateIssueDto? RobotCertificates { get; set; }
  private RobotRenewCertificateRenewDto Settings { get; set; } = new();
  private bool IsError { get; set; } = false;
  private string ReturnPath { get; set; } = "/";

  public readonly List<string> stepHeadings = ["Begin", "Download Cerificates", "Complete"];
  private int currentStep = 0;

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
      NavigationManager.NavigateTo(ReturnPath);
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id))
    {
      var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
      if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("Return", out var param))
      {
        ReturnPath = param[0] ?? string.Empty;
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}
