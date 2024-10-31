using AutoMapper;
using LGDXRobot2Cloud.Data.Models.Identity;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Setting.Roles;

public sealed partial class RolesDetail : ComponentBase
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IRoleService RoleService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public string? Id { get; set; } = null;

  private LgdxRole Role { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  public void TaskAddStep()
  {
    Role.Scopes.Add(string.Empty);
  }

  public void TaskRemoveStep(int i)
  {
    if (Role.Scopes.Count <= 1)
      return;
    Role.Scopes.RemoveAt(i);
  }

  public async Task HandleValidSubmit()
  {
    bool success;

    if (Id != null)
      // Update
      success = await RoleService.UpdateRoleAsync(Id, Mapper.Map<LgdxRoleUpdateDto>(Role));
    else
      // Create
      success = await RoleService.AddRoleAsync(Mapper.Map<LgdxRoleCreateDto>(Role));
    
    if (success)
      NavigationManager.NavigateTo(AppRoutes.Setting.Roles.Index);
    else
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var success = await RoleService.DeleteRoleAsync(Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Setting.Roles.Index);
      else
        IsError = true;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id) && _id != null)
    {
      var user = await RoleService.GetRoleAsync(_id);
      if (user != null) 
      {
        Role = user;
        _editContext = new EditContext(Role);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    else
    {
      Role = new LgdxRole();
      Role.Scopes.Add(string.Empty);
      _editContext = new EditContext(Role);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}