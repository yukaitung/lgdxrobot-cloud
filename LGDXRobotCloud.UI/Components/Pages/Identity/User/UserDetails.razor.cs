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

public partial class UserDetails : ComponentBase
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
  private IDictionary<string,string>? DisableTwoFactorModalErrors { get; set; }
  private IDictionary<string,string>? ResetRecoveryCodesModalErrors { get; set; }
  private List<string>? ResetRecoveryCodes { get; set; } = null;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private EditContext _editContext = null!;
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

  public async Task HandleResetRecoveryCodes()
  {
    try
    {
      var response = await LgdxApiClient.Identity.User.TwoFA.Reset.PostAsync();
      ResetRecoveryCodes = response!.RecoveryCodes!;
    }
    catch (ApiException ex)
    {
      ResetRecoveryCodesModalErrors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleTwoFactorDisable()
  {
    try
    {
      var response = await LgdxApiClient.Identity.User.TwoFA.Disable.PostAsync();
      UserDetailViewModel.TwoFactorEnabled = false;
    }
    catch (ApiException ex)
    {
      DisableTwoFactorModalErrors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleTwoFactorEnable()
  {
    try
    {
      var response = await LgdxApiClient.Identity.User.TwoFA.Enable.PostAsync(new EnableTwoFactorRequestDto{
        TwoFactorCode = UserDetailTwoFactorViewModel.VerficationCode,
      });
      UserDetailTwoFactorViewModel.RecoveryCodes = response!.RecoveryCodes!;
      UserDetailTwoFactorViewModel.Step = 2;
    }
    catch (ApiException ex)
    {
      UserDetailTwoFactorViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleTwoFactorInitiate()
  {
    try
    {
      var response = await LgdxApiClient.Identity.User.TwoFA.Initiate.PostAsync();
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