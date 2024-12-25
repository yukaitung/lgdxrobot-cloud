using System.Text;
using LGDXRobot2Cloud.Data.Models.Identity;
using LGDXRobot2Cloud.UI.Constants;
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

  [SupplyParameterFromQuery]
  private string Token { get; set; } = null!;

  [SupplyParameterFromQuery]
  private string Email { get; set; } = null!;

  [SupplyParameterFromForm]
  public ResetPasswordRequest ResetPasswordRequest { get; set; } = new();

  private bool Success { get; set; } = false;
  private bool IsError { get; set; } = false;

  public async Task HandleResetPassword()
  {
    var result = await AuthService.ResetPasswordAsync(new ResetPasswordRequestDto{
      Email = Email,
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

  protected override Task OnInitializedAsync()
  {
    if (Email == null || Token == null)
    {
      NavigationManager.NavigateTo(AppRoutes.Identity.Login);
      return Task.CompletedTask;
    }
    try 
      {
        var token = Encoding.UTF8.GetString(Convert.FromBase64String(Token));
        Token = token;
      }
      catch (Exception)
      {
        NavigationManager.NavigateTo(AppRoutes.Identity.Login);
      }
    return base.OnInitializedAsync();
  }
}