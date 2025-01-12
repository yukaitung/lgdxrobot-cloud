using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity.Login;

public sealed partial class Login : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [CascadingParameter]
  private HttpContext HttpContext { get; set; } = default!;

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  [SupplyParameterFromForm]
  private LoginViewModel LoginViewModel { get; set; } = new();

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  protected override Task OnInitializedAsync()
  {
    _editContext = new EditContext(LoginViewModel);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    return base.OnInitializedAsync();
  }

  public async Task HandleLogin()
  {
    var loginResponseDto = await LgdxApiClient.Identity.Auth.Login.PostAsync(LoginViewModel.ToLoginRequestDto());
    var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponseDto!.AccessToken);
    var refreshToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponseDto!.RefreshToken);
    var identity = new ClaimsIdentity(accessToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var user = new ClaimsPrincipal(identity);
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
      user, 
      new AuthenticationProperties{
        IsPersistent = false,
        ExpiresUtc = refreshToken.ValidTo
      });
    TokenService.Login(user, loginResponseDto!.AccessToken!, loginResponseDto!.RefreshToken!, accessToken.ValidTo, refreshToken.ValidTo);

    NavigationManager.NavigateTo(ReturnUrl ?? "/");
  }
}