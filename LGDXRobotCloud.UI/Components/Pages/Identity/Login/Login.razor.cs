using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LGDXRobotCloud.UI.Components.Pages.Identity.Login;

public partial class Login : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [CascadingParameter]
  private HttpContext HttpContext { get; set; } = default!;

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  [SupplyParameterFromForm]
  private LoginViewModel? LoginViewModel { get; set; }

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  protected override Task OnInitializedAsync()
  {
    LoginViewModel ??= new LoginViewModel();
    _editContext = new EditContext(LoginViewModel);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    return base.OnInitializedAsync();
  }

  public async Task HandleLogin()
  {
    if (LoginViewModel!.State == LoginViewModelState.TwoFactorCode && LoginViewModel.InputRecoveryCode)
    {
      LoginViewModel.State = LoginViewModelState.TwoFactorRecoveryCode;
      return;
    }

    string? twoFactorCode = null;
    string? twoFactorRecoveryCode = null;
    if (LoginViewModel.State == LoginViewModelState.TwoFactorCode)
    {
      // Generate 2FA code from input
      List<char> codeList = [];
      foreach (string? code in LoginViewModel.TwoFactorCode)
      {
        if (!string.IsNullOrWhiteSpace(code))
        {
          codeList.Add(code[0]);
        }
      }
      twoFactorCode = new string(codeList.ToArray());
    }
    else if (LoginViewModel.State == LoginViewModelState.TwoFactorRecoveryCode)
    {
      twoFactorRecoveryCode = LoginViewModel.TwoFactorRecoveryCode;
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
      // Login Success
      var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse!.AccessToken);
      var refreshToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse!.RefreshToken);
      var identity = new ClaimsIdentity(accessToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
      var user = new ClaimsPrincipal(identity);
      await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        user,
        new AuthenticationProperties
        {
          IsPersistent = false
        });
      TokenService.Login(user, loginResponse!.AccessToken!, loginResponse!.RefreshToken!, accessToken.ValidTo, refreshToken.ValidTo);

      // Setup session data
      var sessionSettings = TokenService.GetSessionSettings(user);
      var realm = await CachedRealmService.GetDefaultRealmAsync();
      TokenService.UpdateSessionSettings(user, new SessionSettings
      {
        CurrentRealmId = realm.Id ?? 0,
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(LoginViewModel.TimeZone ?? "UTC")
      });

      NavigationManager.NavigateTo(string.IsNullOrWhiteSpace(ReturnUrl) ? "/" : ReturnUrl);
    }
    catch (ApiException ex)
    {
      LoginViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }
}