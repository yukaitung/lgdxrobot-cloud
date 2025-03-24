using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LGDXRobotCloud.UI.Components.Pages.Identity.Login;

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

  public void HandleInvalidLogin()
  {
    if (LoginViewModel.TwoFactorCode != null)
    {
      // Reset 2FA
      LoginViewModel.SetupTwoFactor();
    }
  }

  public async Task HandleLogin()
  {
    string? twoFactorCode = null;
    string? twoFactorRecoveryCode = null;
    if (LoginViewModel.TwoFactorCode != null)
    {
      List<char> codeList = new();
      foreach (string? code in LoginViewModel.TwoFactorCode)
      {
        if (!string.IsNullOrWhiteSpace(code))
        {
          codeList.Add(code[0]);
        }
      }
      twoFactorCode = new string(codeList.ToArray());
    }
    try
    {
      var loginResponse = await LgdxApiClient.Identity.Auth.Login.PostAsync(LoginViewModel.ToLoginRequestDto(twoFactorCode, twoFactorRecoveryCode));
      if ((bool)loginResponse!.RequiresTwoFactor!)
      {
        // Show 2FA page
        LoginViewModel.SetupTwoFactor();
        return;
      }
      var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse!.AccessToken);
      var refreshToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse!.RefreshToken);
      var identity = new ClaimsIdentity(accessToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
      var user = new ClaimsPrincipal(identity);
      await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
        user, 
        new AuthenticationProperties{
          IsPersistent = false,
          ExpiresUtc = refreshToken.ValidTo
        });
      TokenService.Login(user, loginResponse!.AccessToken!, loginResponse!.RefreshToken!, accessToken.ValidTo, refreshToken.ValidTo);
      NavigationManager.NavigateTo(ReturnUrl ?? "/");
    }
    catch (ApiException ex)
    {
      LoginViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
      if (LoginViewModel.TwoFactorCode != null)
      {
        // Reset 2FA
        LoginViewModel.SetupTwoFactor();
      }
    }
  }
}