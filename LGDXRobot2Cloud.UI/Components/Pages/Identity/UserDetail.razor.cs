using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using AutoMapper;
using LGDXRobot2Cloud.Data.Models.Identity;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity;

public sealed partial class UserDetail : ComponentBase
{
  [Inject]
  public required IUserService UserService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  private LgdxUser User { get; set; } = new LgdxUser();
  private LgdxUserPassword UserPassword { get; set; } = new LgdxUserPassword();
  private EditContext _editContext = null!;
  private EditContext _editContextPassword = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;
  private bool IsSuccess { get; set; } = false;

  public async Task HandleValidSubmit()
  {
    bool success = await UserService.UpdateUserAsync(Mapper.Map<LgdxUserUpdateDto>(User));
    if (success)
    {
      IsSuccess = true;
      IsError = false;
    }
    else
    {
      IsError = true;
      IsSuccess = false;
    }
  }

  public async Task HandleValidSubmitPassword()
  {
    bool success = await UserService.UpdatePasswordAsync(new UpdatePasswordRequest{
      CurrentPassword = UserPassword.CurrentPassword,
      NewPassword = UserPassword.NewPassword
    });
    if (success)
    {
      IsSuccess = true;
      IsError = false;
    }
    else
    {
      IsError = true;
      IsSuccess = false;
    }
  }

  protected override async Task OnInitializedAsync() 
  {
    _editContextPassword = new EditContext(UserPassword);
    _editContextPassword.SetFieldCssClassProvider(_customFieldClassProvider);
    _editContext = new EditContext(User); // This must go first
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    var user = await UserService.GetUserAsync();
    User = user!;
    _editContext = new EditContext(User);
  }
}