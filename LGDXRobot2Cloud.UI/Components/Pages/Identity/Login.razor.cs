using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Identity;

public sealed partial class Login : ComponentBase
{
  [Inject]
  public required IAuthService AuthService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

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

  public async Task HandleLogin()
  {
    var response = await AuthService.LoginAsync(HttpContext, Mapper.Map<LoginRequestDto>(LoginViewModel));
    if (!response.IsSuccess)
    {
      LoginViewModel.Errors = response?.Errors;
      return;
    }
    NavigationManager.NavigateTo(ReturnUrl ?? "/");
  }
}