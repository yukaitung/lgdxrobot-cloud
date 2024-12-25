using System.Text;
using LGDXRobot2Cloud.Data.Models.Identity;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity;

public sealed partial class ResetPassword : ComponentBase
{
  [Inject] 
  public required IAuthService AuthService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Parameter]
  public string Token { get; set; } = null!;

  [SupplyParameterFromForm]
  public ResetPasswordRequest ResetPasswordRequest { get; set; } = new();

  private bool Success { get; set; } = false;
  private bool IsError { get; set; } = false;

  public async Task HandleResetPassword()
  {
    var result = await AuthService.ResetPasswordAsync(new ResetPasswordRequestDto{
      Email = ResetPasswordRequest.Email,
      Token = Token,
      NewPassword = ResetPasswordRequest.NewPassword,
    });
    if (result)
    {
      Success = true;
      IsError = false;
    }
    else
    {
      IsError = true;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Token), out var _token))
    {
      try 
      {
        var token = Encoding.UTF8.GetString(Convert.FromBase64String(_token!));
        Token = token;
      }
      catch (Exception)
      {
        //NavigationManager.NavigateTo(AppRoutes.Identity.Login);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}