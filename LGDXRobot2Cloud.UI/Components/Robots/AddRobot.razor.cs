using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Data.Models.Blazor;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Robots
{
  public partial class AddRobot
  {
    [Inject]
    public required IRobotService RobotService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }

    [Parameter]
    public EventCallback<(Guid, string, CrudOperation)> OnSubmitDone { get; set; }

    private RobotBlazor Robot { get; set; } = null!;
    private RobotCreateResponseDto? RobotCertificates { get; set; }
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    private readonly List<string> stepHeadings = ["Information", "Download Cerificates", "Complete"];
    private int currentStep = 0;

    protected async Task HandleValidSubmit()
    {
      if (currentStep == 0)
      {
        var success = await RobotService.AddRobotAsync(Mapper.Map<RobotCreateDto>(Robot));
        if (success != null)
        {
          RobotCertificates = success;
          IsError = false;
          IsInvalid = false;
          currentStep++;
        }
        else
          IsError = true;
      }
      else if (currentStep == 1)
      {
        await OnSubmitDone.InvokeAsync((RobotCertificates!.Id, RobotCertificates!.Name, CrudOperation.Create));
        RobotCertificates = null;
        currentStep++;
      }
      else 
      {
        Robot = new RobotBlazor();
        currentStep = 0;
      }
    }

    protected void HandleInvalidSubmit()
    {
      IsInvalid = true;
    }

    protected override void OnInitialized()
    {
      Robot = new RobotBlazor();
      _editContext = new EditContext(Robot);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
  }
}