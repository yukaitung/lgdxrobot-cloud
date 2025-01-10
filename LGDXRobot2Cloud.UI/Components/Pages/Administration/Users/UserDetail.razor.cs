using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Administration.Users;

public sealed partial class UserDetail : ComponentBase, IDisposable
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IUsersService UsersService { get; set; }

  [Inject]
  public required IRoleService RoleService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public string? Id { get; set; } = null;

  private DotNetObjectReference<UserDetail> ObjectReference = null!;
  private UserDetailViewModel UserDetailViewModel { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  // Form helping variables
  private readonly string[] AdvanceSelectElements = ["RoleId-"];
  private int InitaisedAdvanceSelect { get; set; } = 0;

  [JSInvokable("HandlSelectSearch")]
  public async Task HandlSelectSearch(string elementId, string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    var index = elementId.IndexOf('-');
    if (index == -1 || index + 1 == elementId.Length)
      return;
    string element = elementId[..(index + 1)];
    if (element == AdvanceSelectElements[0])
    {
      var result = await RoleService.SearchRolesAsync(name);
      var str = result.Data;
      await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, str);
    }
  }

  [JSInvokable("HandleSelectChange")]
  public void HandleSelectChange(string elementId, string? id, string? name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    var index = elementId.IndexOf('-');
    if (index == -1 || index + 1 == elementId.Length)
      return;
    string element = elementId[..(index + 1)];
    int order = int.Parse(elementId[(index + 1)..]);
    if (element == AdvanceSelectElements[0])
    {
      UserDetailViewModel.Roles[order] = name;
    }
  }

  public void ListAddRole()
  {
    UserDetailViewModel.Roles.Add(string.Empty);
  }

  public async Task ListRemoveRole(int i)
  {
    if (UserDetailViewModel.Roles.Count <= 1)
      return;
    if (i < UserDetailViewModel.Roles.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1, true);
    UserDetailViewModel.Roles.RemoveAt(i);
    InitaisedAdvanceSelect--;
  }

  public async Task HandleValidSubmit()
  {
    ApiResponse<bool> response;
    if (Id != null)
      // Update
      response = await UsersService.UpdateUserAsync(Id, Mapper.Map<LgdxUserUpdateAdminDto>(UserDetailViewModel));
    else
      // Create
      response = await UsersService.AddUserAsync(Mapper.Map<LgdxUserCreateAdminDto>(UserDetailViewModel));
    
    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Administration.Users.Index);
    else
      UserDetailViewModel.Errors = response.Errors;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await UsersService.DeleteUserAsync(Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Administration.Users.Index);
      else
        UserDetailViewModel.Errors = response.Errors;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id) && _id != null)
    {
      var response = await UsersService.GetUserAsync(_id);
      var user = response.Data;
      if (user != null) 
      {
        UserDetailViewModel = Mapper.Map<UserDetailViewModel>(user);
        _editContext = new EditContext(UserDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    else
    {
      UserDetailViewModel = new UserDetailViewModel();
      UserDetailViewModel.Roles.Add(string.Empty);
      _editContext = new EditContext(UserDetailViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender)
    {
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitDotNet", ObjectReference);
    }
    if (InitaisedAdvanceSelect < UserDetailViewModel.Roles.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        UserDetailViewModel.Roles.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = UserDetailViewModel.Roles.Count;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}