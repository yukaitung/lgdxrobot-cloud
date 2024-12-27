using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity;

public sealed partial class ForgetPassword : ComponentBase
{
  [Inject] 
  public required IAuthService AuthService { get; set; }

  [SupplyParameterFromForm]
  public ForgotPasswordRequestDto ForgotPasswordRequest { get; set; } = null!;

  private bool Success { get; set; } = false;

  public async Task HandleForgotPassword()
  {
    await AuthService.ForgotPasswordAsync(ForgotPasswordRequest);
    Success = true;
  }
}