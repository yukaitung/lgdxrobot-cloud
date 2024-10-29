using LGDXRobot2Cloud.Data.Models.Identity;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity;

public sealed partial class Login : ComponentBase
{
  [Inject] 
  public required IUserService UserService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; }

  [CascadingParameter]
  private HttpContext HttpContext { get; set; } = default!;

  [SupplyParameterFromForm]
  public LoginRequest LoginRequest { get; set; } = new();

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  private bool IsError { get; set; } = false;

  public async Task HandleLogin()
  {
    var request = await UserService.LoginAsync(HttpContext, LoginRequest);
    if (!request)
    {
      IsError = true;
      return;
    }
    
    NavigationManager.NavigateTo(ReturnUrl ?? "/");
  }
}