using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity.User;

public sealed partial class UserDetail : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  private UserDetailViewModel UserDetailViewModel { get; set; } = new();
  private UserDetailPasswordViewModel UserDetailPasswordViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private EditContext _editContextPassword = null!;

  public async Task HandleValidSubmit()
  {
    await LgdxApiClient.Identity.User.PutAsync(UserDetailViewModel.ToUpdateDto());
  }

  public async Task HandleValidSubmitPassword()
  {
    await LgdxApiClient.Identity.User.Password.PostAsync(UserDetailPasswordViewModel.ToUpdateDto());
  }

  protected override async Task OnInitializedAsync() 
  {
    _editContextPassword = new EditContext(UserDetailPasswordViewModel);
    _editContextPassword.SetFieldCssClassProvider(_customFieldClassProvider);
    _editContext = new EditContext(UserDetailViewModel); // This must go first
    var user = await LgdxApiClient.Identity.User.GetAsync();
    UserDetailViewModel.FromDto(user!);
    _editContext = new EditContext(UserDetailViewModel);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
  }
}