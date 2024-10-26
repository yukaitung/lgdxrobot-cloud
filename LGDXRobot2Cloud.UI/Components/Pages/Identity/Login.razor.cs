using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LGDXRobot2Cloud.Data.Models.Identity;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity;

public sealed partial class Login : ComponentBase
{
  [Inject] 
  public required IUserService UserService { get; set; }

  [CascadingParameter]
  private HttpContext HttpContext { get; set; } = default!;

  [SupplyParameterFromForm]
  public LoginRequest LoginRequest { get; set; } = new();

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  private bool IsError { get; set; } = false;

  public async Task HandleLogin()
  {
    var loginResponse = await UserService.LoginAsync(LoginRequest);
    if (loginResponse == null)
    {
      IsError = true;
      return;
    }
    var token = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse!.AccessToken);
    var identity = new ClaimsIdentity([], CookieAuthenticationDefaults.AuthenticationScheme);
    identity.AddClaims(token.Claims);
    var user = new ClaimsPrincipal(identity);
    var authProperties = new AuthenticationProperties{};
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user, authProperties);
  }
}