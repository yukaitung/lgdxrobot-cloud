using AutoMapper;
using LGDXRobot2Cloud.Data.Models.Identity;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Setting.Users;

public sealed partial class UsersDetail : ComponentBase, IDisposable
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

  private DotNetObjectReference<UsersDetail> ObjectReference = null!;
  private LgdxUser User { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

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
      await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, result);
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
      User.Roles[order] = name;
    }
  }

  public void TaskAddStep()
  {
    User.Roles.Add(string.Empty);
  }

  public async Task TaskRemoveStep(int i)
  {
    if (User.Roles.Count <= 1)
      return;
    if (i < User.Roles.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1, true);
    User.Roles.RemoveAt(i);
    InitaisedAdvanceSelect--;
  }

  public async Task HandleValidSubmit()
  {
    bool success;

    if (Id != null)
      // Update
      success = await UsersService.UpdateUserAsync(Id, Mapper.Map<LgdxUserUpdateDto>(User));
    else
      // Create
      success = await UsersService.AddUserAsync(Mapper.Map<LgdxUserCreateDto>(User));
    
    if (success)
      NavigationManager.NavigateTo(AppRoutes.Setting.Users.Index);
    else
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var success = await UsersService.DeleteUserAsync(Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Setting.Users.Index);
      else
        IsError = true;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id) && _id != null)
    {
      var user = await UsersService.GetUserAsync(_id);
      if (user != null) 
      {
        User = user;
        _editContext = new EditContext(User);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    else
    {
      User = new LgdxUser();
      User.Roles.Add(string.Empty);
      _editContext = new EditContext(User);
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
    if (InitaisedAdvanceSelect < User.Roles.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        User.Roles.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = User.Roles.Count;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}