using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity;

public sealed partial class ForgetPassword : ComponentBase
{
  [Inject] 
  public required IAuthService AuthService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [SupplyParameterFromForm]
  private ForgotPasswordViewModel ForgotPasswordViewModel { get; set; } = new();

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  protected override Task OnInitializedAsync()
  {
    _editContext = new EditContext(ForgotPasswordViewModel);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    return base.OnInitializedAsync();
  }

  public async Task HandleForgotPassword()
  {
    var result = await AuthService.ForgotPasswordAsync(Mapper.Map<ForgotPasswordRequestDto>(ForgotPasswordViewModel));
    if (result.IsSuccess)
    {
      ForgotPasswordViewModel.Success = true;
    }
    else
    {
      ForgotPasswordViewModel.Errors = result.Errors;
    }
  }
}