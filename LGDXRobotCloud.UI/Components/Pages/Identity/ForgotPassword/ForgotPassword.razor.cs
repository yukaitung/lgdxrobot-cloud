using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Identity.ForgotPassword;

public partial class ForgotPassword : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }
  
  [SupplyParameterFromForm]
  private ForgotPasswordViewModel? ForgotPasswordViewModel { get; set; }

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  
  public async Task HandleForgotPassword()
  {
    try
    {
      await LgdxApiClient.Identity.Auth.ForgotPassword.PostAsync(ForgotPasswordViewModel!.ToForgotPasswordRequestDto());
      ForgotPasswordViewModel!.IsSuccess = true;
    }
    catch (ApiException ex)
    {
      ForgotPasswordViewModel!.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  protected override Task OnInitializedAsync()
  {
    ForgotPasswordViewModel ??= new ForgotPasswordViewModel();
    _editContext = new EditContext(ForgotPasswordViewModel);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    return base.OnInitializedAsync();
  }
}