using AutoMapper;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
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
    public EventCallback<(int, string, CrudOperation)> OnSubmitDone { get; set; }

    private RobotBlazor Robot { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    protected async Task HandleValidSubmit()
    {
      var success = await RobotService.AddRobotAsync(Mapper.Map<RobotCreateDto>(Robot));
      if (success != null)
      {
        await JSRuntime.InvokeVoidAsync("CloseModal", "addRobotModal");
        await OnSubmitDone.InvokeAsync((success.Id, success.Name, CrudOperation.Create));
        Robot = new RobotBlazor();
      }
      else
        IsError = true;
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