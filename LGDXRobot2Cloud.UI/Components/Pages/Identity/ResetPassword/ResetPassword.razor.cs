using System.Text;
using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity.ResetPassword;

public sealed partial class ResetPassword : ComponentBase
{
  [Inject] 
  public required IAuthService AuthService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

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
    var result = await AuthService.ResetPasswordAsync(Mapper.Map<ResetPasswordRequestDto>(ResetPasswordViewModel));
    if (result.IsSuccess)
    {
      ResetPasswordViewModel.IsSuccess = true;
    }
    else
    {
      ResetPasswordViewModel.Errors = result.Errors;
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