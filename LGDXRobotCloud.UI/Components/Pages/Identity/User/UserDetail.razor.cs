using System.Globalization;
using System.Text.Encodings.Web;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;
using Net.Codecrete.QrCodeGenerator;

namespace LGDXRobotCloud.UI.Components.Pages.Identity.User;

public sealed partial class UserDetail : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required UrlEncoder UrlEncoder { get; set; }

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  private UserDetailViewModel UserDetailViewModel { get; set; } = new();
  private UserDetailPasswordViewModel UserDetailPasswordViewModel { get; set; } = new();
  private UserDetailTwoFactorViewModel UserDetailTwoFactorViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private EditContext _editContextPassword = null!;
  private EditContext _editContextTwoFactor = null!;

  private string GenerateQrCodeUri(string email, string unformattedKey)
  {
    return string.Format(
      CultureInfo.InvariantCulture,
      "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
      UrlEncoder.Encode("LGDXRobot Cloud"),
      UrlEncoder.Encode(email),
      unformattedKey);
  }

  public async Task HandleValidSubmit()
  {
    try
    {
      await LgdxApiClient.Identity.User.PutAsync(UserDetailViewModel.ToUpdateDto());
    }
    catch (ApiException ex)
    {
      UserDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleValidSubmitPassword()
  {
    try
    {
      await LgdxApiClient.Identity.User.Password.PostAsync(UserDetailPasswordViewModel.ToUpdateDto());
    }
    catch (ApiException ex)
    {
      UserDetailPasswordViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleResetTwoFactor()
  {
    try
    {
      var response = await LgdxApiClient.Identity.User.TwoFactor.PostAsync(new TwoFactorRequestDto{
        ResetSharedKey = true
      });
      UserDetailViewModel.TwoFactorEnabled = (bool)response!.IsTwoFactorEnabled!;
    }
    catch (ApiException ex)
    {
      UserDetailTwoFactorViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleValidSubmitTwoFactor()
  {
    try
    {
      var response = await LgdxApiClient.Identity.User.TwoFactor.PostAsync(new TwoFactorRequestDto{
        TwoFactorCode = UserDetailTwoFactorViewModel.VerficationCode,
        Enable = true
      });
      UserDetailTwoFactorViewModel.RecoveryCodes = response!.RecoveryCodes!;
      UserDetailTwoFactorViewModel.Step = 2;
    }
    catch (ApiException ex)
    {
      UserDetailTwoFactorViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleStart2FASetup()
  {
    try
    {
      var response = await LgdxApiClient.Identity.User.TwoFactor.PostAsync(new TwoFactorRequestDto());
      UserDetailTwoFactorViewModel.SharedKey = response!.SharedKey!;
      var uri = GenerateQrCodeUri(UserDetailViewModel.UserName, response!.SharedKey!);
      var qr = QrCode.EncodeText(uri, QrCode.Ecc.Medium);
      UserDetailTwoFactorViewModel.SvgGraphicsPath = qr.ToGraphicsPath();
      UserDetailTwoFactorViewModel.Step = 1;
    }
    catch (ApiException ex)
    {
      UserDetailTwoFactorViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  protected override async Task OnInitializedAsync() 
  {
    _editContextTwoFactor = new EditContext(UserDetailTwoFactorViewModel);
    _editContextTwoFactor.SetFieldCssClassProvider(_customFieldClassProvider);
    _editContextPassword = new EditContext(UserDetailPasswordViewModel);
    _editContextPassword.SetFieldCssClassProvider(_customFieldClassProvider);
    _editContext = new EditContext(UserDetailViewModel); // This must go first
    var user = await LgdxApiClient.Identity.User.GetAsync();
    UserDetailViewModel.FromDto(user!);
    _editContext = new EditContext(UserDetailViewModel);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
  }
}