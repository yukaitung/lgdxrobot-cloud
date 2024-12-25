using LGDXRobot2Cloud.Data.Models.Identity;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity;

public sealed partial class ForgetPassword : ComponentBase
{
  [Inject] 
  public required IAuthService AuthService { get; set; }

  [SupplyParameterFromForm]
  public ForgotPasswordRequestDto ForgotPasswordRequest { get; set; } = new();

  private bool Success { get; set; } = false;

  public async Task HandleForgotPassword()
  {
    await AuthService.ForgotPasswordAsync(ForgotPasswordRequest);
    Success = true;
  }
}