using System.Text;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Identity.ResetPassword;

public sealed partial class ResetPassword : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [SupplyParameterFromQuery]
  private string Token { get; set; } = null!;

  [SupplyParameterFromQuery]
  private string Email { get; set; } = null!;

  [SupplyParameterFromQuery]
  private bool NewUser { get; set; } = false;

  [SupplyParameterFromForm]
  public ResetPasswordViewModel ResetPasswordViewModel { get; set; } = new();

  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleResetPassword()
  {
    try
    {
      await LgdxApiClient.Identity.Auth.ResetPassword.PostAsync(ResetPasswordViewModel.ToResetPasswordRequestDto());
      ResetPasswordViewModel.IsSuccess = true;
    }
    catch (ApiException ex)
    {
      ResetPasswordViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
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
      ResetPasswordViewModel.Email = Email;
      var token = Encoding.UTF8.GetString(Convert.FromBase64String(Token));
      ResetPasswordViewModel.Token = token;
    }
    catch (Exception)
    {
      NavigationManager.NavigateTo(AppRoutes.Identity.Login);
    }
    _editContext = new EditContext(ResetPasswordViewModel);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    return base.OnInitializedAsync();
  }
}