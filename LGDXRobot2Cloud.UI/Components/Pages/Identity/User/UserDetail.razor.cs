using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using AutoMapper;
using LGDXRobot2Cloud.UI.ViewModels.Identity;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity.User;

public sealed partial class UserDetail : ComponentBase
{
  [Inject]
  public required IUserService UserService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  private UserDetailViewModel UserDetailViewModel { get; set; } = new();
  private UserDetailPasswordViewModel UserDetailPasswordViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private EditContext _editContextPassword = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public async Task HandleValidSubmit()
  {
    var respone = await UserService.UpdateUserAsync(Mapper.Map<LgdxUserUpdateDto>(UserDetailViewModel));
    if (respone.IsSuccess)
    {
      UserDetailViewModel.Errors = null;
      UserDetailViewModel.IsSuccess = true;
    }
    else
    {
      UserDetailViewModel.Errors = respone.Errors;
      UserDetailViewModel.IsSuccess = false;
    }
  }

  public async Task HandleValidSubmitPassword()
  {
    var respone = await UserService.UpdatePasswordAsync(Mapper.Map<UpdatePasswordRequestDto>(UserDetailPasswordViewModel));
    if (respone.IsSuccess)
    {
      UserDetailPasswordViewModel.Errors = null;
      UserDetailPasswordViewModel.IsSuccess = true;
    }
    else
    {
      UserDetailPasswordViewModel.Errors = respone.Errors;
      UserDetailPasswordViewModel.IsSuccess = false;
    }
  }

  protected override async Task OnInitializedAsync() 
  {
    _editContextPassword = new EditContext(UserDetailPasswordViewModel);
    _editContextPassword.SetFieldCssClassProvider(_customFieldClassProvider);
    _editContext = new EditContext(UserDetailViewModel); // This must go first
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    var response = await UserService.GetUserAsync();
    UserDetailViewModel = Mapper.Map<UserDetailViewModel>(response.Data);
    _editContext = new EditContext(UserDetailViewModel);
  }
}